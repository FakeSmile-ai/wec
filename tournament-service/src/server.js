const express = require('express');
const cors = require('cors');

const app = express();
app.use(express.json({ limit: '1mb' }));
app.use(cors());

const TOURNAMENT_ID = 'cup-current';
const TEAMS_SERVICE_BASE_URL =
  process.env.TEAMS_SERVICE_BASE_URL || 'http://teams-service:8082';
const MATCHES_SERVICE_BASE_URL =
  process.env.MATCHES_SERVICE_BASE_URL || 'http://matches-service:8081';
const PORT = parseInt(process.env.PORT || '8083', 10);
const CACHE_TTL_MS = 45 * 1000;

const STATUS_LABELS = {
  scheduled: 'Programado',
  live: 'En juego',
  finished: 'Finalizado'
};

const tournamentConfig = {
  id: TOURNAMENT_ID,
  groups: {
    'group-a': [null, null],
    'group-b': [null, null]
  },
  finalMatchId: null
};

let tournamentCache = null;

function resetCache() {
  tournamentCache = null;
}

/* ------------------------------------------------------- *
 * Helpers
 * ------------------------------------------------------- */

function toPalette(index) {
  const swatches = [
    { primary: '#2563eb', secondary: '#60a5fa' },
    { primary: '#f97316', secondary: '#fed7aa' },
    { primary: '#10b981', secondary: '#6ee7b7' },
    { primary: '#7c3aed', secondary: '#c4b5fd' },
    { primary: '#14b8a6', secondary: '#5eead4' },
    { primary: '#0ea5e9', secondary: '#93c5fd' },
    { primary: '#f87171', secondary: '#fecaca' },
    { primary: '#a855f7', secondary: '#d8b4fe' }
  ];
  return swatches[index % swatches.length];
}

function normalizeStatus(raw) {
  const value = (raw || '').toString().toLowerCase();
  if (value === 'live' || value === 'finished' || value === 'scheduled') {
    return value;
  }
  return 'scheduled';
}

function formatScheduleLabel(date) {
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) {
    return 'Sin fecha';
  }
  return new Intl.DateTimeFormat('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(date);
}

function formatUpdatedLabel(date) {
  return `Actualizado ${new Intl.DateTimeFormat('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(date)}`;
}

function placeholderSlot(label) {
  return {
    id: null,
    displayName: label,
    score: null,
    isPlaceholder: true,
    detail: null,
    palette: null
  };
}

function createTeamDetail(raw, index) {
  const palette = toPalette(index);
  return {
    id: String(raw.id),
    name: raw.name || `Equipo ${raw.id}`,
    shortName: (raw.acronym || raw.name || `EQ${raw.id}`)
      .slice(0, 3)
      .toUpperCase(),
    city: raw.city || null,
    coach: raw.coach || null,
    seed: index + 1,
    record: '0-0',
    streak: 'N/A',
    palette,
    narrative: `El conjunto ${raw.name || raw.id} representa a ${
      raw.city || 'la liga'
    } dirigido por ${raw.coach || 'su cuerpo técnico actual'}.`,
    players: [
      `${raw.name || raw.id} Player 1`,
      `${raw.name || raw.id} Player 2`,
      `${raw.name || raw.id} Player 3`
    ],
    stats: [
      { label: 'PPG', value: (70 + index * 2).toFixed(1) },
      { label: 'RPG', value: (34 + index).toFixed(1) },
      { label: 'APG', value: (18 + index).toFixed(1) }
    ]
  };
}

function teamSlot(detail) {
  return {
    id: detail.id,
    displayName: detail.name,
    shortName: detail.shortName,
    seed: detail.seed,
    record: detail.record,
    score: null,
    isPlaceholder: false,
    originLabel: null,
    detail,
    palette: detail.palette
  };
}

async function fetchJson(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(
      `Request to ${url} failed with ${response.status}: ${text}`.trim()
    );
  }
  return response.json();
}

async function fetchTeams() {
  const url = new URL('/api/teams', TEAMS_SERVICE_BASE_URL);
  url.searchParams.set('page', '0');
  url.searchParams.set('size', '200');

  const payload = await fetchJson(url);
  if (Array.isArray(payload)) return payload;
  if (Array.isArray(payload?.content)) return payload.content;
  if (Array.isArray(payload?.data)) return payload.data;
  return [];
}

async function fetchMatchById(id) {
  const url = new URL(`/api/matches/${id}`, MATCHES_SERVICE_BASE_URL);
  return fetchJson(url);
}

async function scheduleMatch(homeTeamId, awayTeamId, scheduledAt) {
  const url = new URL('/api/matches/programar', MATCHES_SERVICE_BASE_URL);
  const iso = scheduledAt.toISOString();
  const [date, timeWithMs] = iso.split('T');
  const time = timeWithMs.slice(0, 5);
  const payload = {
    homeTeamId,
    awayTeamId,
    date,
    time,
    quarterDurationSeconds: 600
  };

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (!response.ok) {
      const text = await response.text().catch(() => '');
      console.warn(
        `[tournament-service] matches-service responded ${response.status}: ${text}`
      );
      return null;
    }
    const match = await response.json();
    return match?.id ?? null;
  } catch (error) {
    console.warn(
      '[tournament-service] unable to schedule match',
      error?.message || error
    );
    return null;
  }
}

async function finishMatchInMatchesService(matchId, scoreA, scoreB) {
  if (!matchId) return;
  const url = new URL(`/api/matches/${matchId}/finish`, MATCHES_SERVICE_BASE_URL);
  try {
    await fetch(url, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        homeScore: scoreA ?? undefined,
        awayScore: scoreB ?? undefined
      })
    });
  } catch (error) {
    console.warn(
      `[tournament-service] finish match ${matchId} failed`,
      error?.message || error
    );
  }
}

function createMatchView(match, teamDetailsMap, { groupId, slotIndex }) {
  const status = normalizeStatus(match.status);
  const statusLabel = STATUS_LABELS[status] ?? STATUS_LABELS.scheduled;

  const homeDetail =
    teamDetailsMap.get(String(match.homeTeamId)) ||
    createTeamDetail(
      {
        id: match.homeTeamId,
        name: match.homeTeamName || `Equipo ${match.homeTeamId}`,
        city: null,
        coach: null
      },
      teamDetailsMap.size
    );
  const awayDetail =
    teamDetailsMap.get(String(match.awayTeamId)) ||
    createTeamDetail(
      {
        id: match.awayTeamId,
        name: match.awayTeamName || `Equipo ${match.awayTeamId}`,
        city: null,
        coach: null
      },
      teamDetailsMap.size + 1
    );

  teamDetailsMap.set(homeDetail.id, homeDetail);
  teamDetailsMap.set(awayDetail.id, awayDetail);

  const scheduledAt = match.dateTime ? new Date(match.dateTime) : null;
  const scheduleLabel = scheduledAt
    ? formatScheduleLabel(scheduledAt)
    : 'Sin fecha';

  const view = {
    id: String(match.id),
    label: match.label || `Partido ${match.id}`,
    round: match.round || 'group',
    status,
    statusLabel,
    scheduleLabel,
    scheduledAtUtc: scheduledAt ? scheduledAt.toISOString() : null,
    venue: match.venue || 'Estadio Principal',
    broadcast: match.broadcast || null,
    teamA: {
      ...teamSlot(homeDetail),
      score:
        typeof match.homeScore === 'number' ? match.homeScore : null
    },
    teamB: {
      ...teamSlot(awayDetail),
      score:
        typeof match.awayScore === 'number' ? match.awayScore : null
    },
    winnerId: null,
    groupId,
    slotIndex,
    isPlaceholder: false
  };

  if (
    status === 'finished' &&
    view.teamA.score !== null &&
    view.teamB.score !== null
  ) {
    if (view.teamA.score > view.teamB.score) {
      view.winnerId = view.teamA.id;
    } else if (view.teamB.score > view.teamA.score) {
      view.winnerId = view.teamB.id;
    }
  }

  return view;
}

function createPlaceholderView(label, { groupId, slotIndex }) {
  return {
    id: `${groupId}-slot-${slotIndex}`,
    label,
    round: 'group',
    status: 'scheduled',
    statusLabel: 'Sin asignar',
    scheduleLabel: 'Selecciona un partido',
    scheduledAtUtc: null,
    venue: 'Por definir',
    broadcast: null,
    teamA: placeholderSlot('Equipo A'),
    teamB: placeholderSlot('Equipo B'),
    winnerId: null,
    groupId,
    slotIndex,
    isPlaceholder: true
  };
}

function determineGroupResults(matchViews) {
  const winners = [];
  const seen = new Set();

  for (const match of matchViews) {
    if (
      match.isPlaceholder ||
      match.status !== 'finished' ||
      match.teamA.score === null ||
      match.teamB.score === null
    ) {
      continue;
    }

    const winner =
      match.teamA.score > match.teamB.score
        ? match.teamA
        : match.teamB.score > match.teamA.score
        ? match.teamB
        : null;
    if (winner && !seen.has(winner.id)) {
      winners.push(winner);
      seen.add(winner.id);
    }
  }

  const champion = winners.length > 0 ? winners[winners.length - 1] : null;
  return { winners, champion };
}

async function buildFinalView({
  championA,
  championB,
  existingFinalMatchId,
  teamDetailsMap,
  referenceDate
}) {
  if (!championA || !championB || championA.id === championB.id) {
    return {
      view: {
        id: 'final-slot',
        label: 'Final',
        round: 'final',
        status: 'scheduled',
        statusLabel: 'Esperando ganadores',
        scheduleLabel: 'Pendiente',
        scheduledAtUtc: null,
        venue: 'Arena Nacional',
        broadcast: null,
        teamA: championA ?? placeholderSlot('Ganador Grupo A'),
        teamB: championB ?? placeholderSlot('Ganador Grupo B'),
        winnerId: null,
        groupId: 'final',
        slotIndex: 0,
        isPlaceholder: true
      },
      serviceMatchId: existingFinalMatchId
    };
  }

  if (!existingFinalMatchId) {
    const scheduledAt = new Date(referenceDate.getTime() + 2 * 24 * 60 * 60 * 1000);
    const newMatchId = await scheduleMatch(
      Number(championA.id),
      Number(championB.id),
      scheduledAt
    );
    if (newMatchId) {
      tournamentConfig.finalMatchId = newMatchId;
      resetCache();
      return buildFinalView({
        championA,
        championB,
        existingFinalMatchId: newMatchId,
        teamDetailsMap,
        referenceDate: scheduledAt
      });
    }

    return {
      view: {
        id: 'final-slot',
        label: 'Final',
        round: 'final',
        status: 'scheduled',
        statusLabel: 'Programar final',
        scheduleLabel: 'Selecciona partido',
        scheduledAtUtc: null,
        venue: 'Arena Nacional',
        broadcast: null,
        teamA: championA,
        teamB: championB,
        winnerId: null,
        groupId: 'final',
        slotIndex: 0,
        isPlaceholder: true
      },
      serviceMatchId: null
    };
  }

  try {
    const record = await fetchMatchById(existingFinalMatchId);
    const view = createMatchView(record, teamDetailsMap, {
      groupId: 'final',
      slotIndex: 0
    });
    view.round = 'final';
    view.label = view.label || 'Final';
    view.isPlaceholder = false;
    return { view, serviceMatchId: existingFinalMatchId };
  } catch (error) {
    console.warn(
      '[tournament-service] final match fetch failed',
      error?.message || error
    );
    return {
      view: {
        id: 'final-slot',
        label: 'Final',
        round: 'final',
        status: 'scheduled',
        statusLabel: 'Programar final',
        scheduleLabel: 'Selecciona partido',
        scheduledAtUtc: null,
        venue: 'Arena Nacional',
        broadcast: null,
        teamA: championA,
        teamB: championB,
        winnerId: null,
        groupId: 'final',
        slotIndex: 0,
        isPlaceholder: true
      },
      serviceMatchId: null
    };
  }
}

async function buildTournamentState(force = false) {
  if (
    !force &&
    tournamentCache &&
    Date.now() - tournamentCache.createdAt < CACHE_TTL_MS
  ) {
    return tournamentCache;
  }

  const teams = await fetchTeams();
  const teamDetailsMap = new Map();
  teams.forEach((team, index) =>
    teamDetailsMap.set(String(team.id), createTeamDetail(team, index))
  );

  const groups = [];
  const actualMatches = [];
  const finishedMatches = [];
  const groupChampions = {};

  for (const [groupId, slots] of Object.entries(tournamentConfig.groups)) {
    const matchViews = [];
    for (let slotIndex = 0; slotIndex < slots.length; slotIndex += 1) {
      const matchId = slots[slotIndex];
      if (!matchId) {
        matchViews.push(
          createPlaceholderView(`Slot ${slotIndex + 1}`, {
            groupId,
            slotIndex
          })
        );
        continue;
      }

      try {
        const record = await fetchMatchById(matchId);
        const view = createMatchView(record, teamDetailsMap, {
          groupId,
          slotIndex
        });
        matchViews.push(view);
        actualMatches.push(view);
        if (view.status === 'finished') {
          finishedMatches.push(view);
        }
      } catch (error) {
        console.warn(
          '[tournament-service] failed to fetch match',
          matchId,
          error?.message || error
        );
        matchViews.push(
          createPlaceholderView(`Slot ${slotIndex + 1}`, {
            groupId,
            slotIndex
          })
        );
      }
    }

    const result = determineGroupResults(matchViews);
    groupChampions[groupId] = result.champion;

    groups.push({
      id: groupId,
      name: groupId === 'group-a' ? 'Grupo A' : 'Grupo B',
      color: toPalette(groupId === 'group-a' ? 0 : 1).secondary,
      matches: matchViews,
      qualifiers: result.winners,
      semiFinal: null
    });
  }

  const championA = groupChampions['group-a'] ?? null;
  const championB = groupChampions['group-b'] ?? null;

  const referenceDate = actualMatches
    .map((match) =>
      match.scheduledAtUtc ? new Date(match.scheduledAtUtc).getTime() : 0
    )
    .reduce((max, value) => Math.max(max, value || 0), Date.now());

  const finalInfo = await buildFinalView({
    championA,
    championB,
    existingFinalMatchId: tournamentConfig.finalMatchId,
    teamDetailsMap,
    referenceDate: new Date(referenceDate || Date.now())
  });

  const finalView = finalInfo.view;
  if (finalInfo.serviceMatchId && finalInfo.serviceMatchId !== tournamentConfig.finalMatchId) {
    tournamentConfig.finalMatchId = finalInfo.serviceMatchId;
  }

  if (!finalView.isPlaceholder) {
    actualMatches.push(finalView);
    if (finalView.status === 'finished') {
      finishedMatches.push(finalView);
    }
  }

  const matchesPlayed = finishedMatches.length;
  const totalMatches = actualMatches.length;
  const progress =
    totalMatches > 0 ? matchesPlayed / totalMatches : 0;

  const winner =
    finalView.status === 'finished' && finalView.winnerId
      ? finalView.winnerId === finalView.teamA.id
        ? finalView.teamA
        : finalView.teamB
      : null;

  const teamsIndex = Object.fromEntries(teamDetailsMap);

  const detail = {
    id: TOURNAMENT_ID,
    code: 'CUP-2025',
    name: 'Copa Invitacional',
    heroTitle: 'Copa Invitacional 2025',
    season: '2025',
    location: 'Sedes Oficiales',
    venue: 'Arena Nacional',
    description:
      'Torneo armado con partidos programados en el servicio de partidos.',
    scheduleLabel: `${new Intl.DateTimeFormat('es-ES', { month: 'short' }).format(
      new Date()
    )} ${new Date().getFullYear()} - ${new Intl.DateTimeFormat('es-ES', {
      month: 'short'
    }).format(new Date(Date.now() + 14 * 24 * 60 * 60 * 1000))} ${new Date().getFullYear()}`,
    updatedLabel: formatUpdatedLabel(new Date()),
    domain: 'scoreboard.local',
    progress,
    matchesPlayed,
    totalMatches,
    summary:
      'Selecciona dos partidos por grupo y deja que los ganadores se enfrenten en la final.',
    groups,
    final: finalView,
    winner,
    teams: Object.values(teamsIndex),
    teamsIndex
  };

  const summary = {
    id: detail.id,
    code: detail.code,
    name: detail.name,
    season: detail.season,
    heroTitle: detail.heroTitle,
    location: detail.location,
    scheduleLabel: detail.scheduleLabel,
    progress: detail.progress,
    matchesPlayed: detail.matchesPlayed,
    totalMatches: detail.totalMatches
  };

  tournamentCache = {
    summary,
    detail,
    createdAt: Date.now()
  };

  return tournamentCache;
}

async function ensureState(force = false) {
  return buildTournamentState(force);
}

async function getAssignedMatchRecords({
  excludeGroup = null,
  excludeSlot = null,
  excludeFinal = false
} = {}) {
  const records = [];

  for (const [groupId, slots] of Object.entries(tournamentConfig.groups)) {
    for (let slotIndex = 0; slotIndex < slots.length; slotIndex += 1) {
      if (groupId === excludeGroup && slotIndex === excludeSlot) continue;
      const matchId = slots[slotIndex];
      if (!matchId) continue;
      try {
        const record = await fetchMatchById(matchId);
        records.push(record);
      } catch (error) {
        console.warn(
          '[tournament-service] validation fetch failed',
          matchId,
          error?.message || error
        );
      }
    }
  }

  if (
    !excludeFinal &&
    tournamentConfig.finalMatchId &&
    tournamentConfig.finalMatchId !== null
  ) {
    try {
      const record = await fetchMatchById(tournamentConfig.finalMatchId);
      records.push(record);
    } catch (error) {
      console.warn(
        '[tournament-service] final validation fetch failed',
        tournamentConfig.finalMatchId,
        error?.message || error
      );
    }
  }

  return records;
}

async function validateAssignment(groupId, slotIndex, matchId) {
  if (!Number.isInteger(matchId) || matchId <= 0) {
    throw new Error('Identificador de partido inválido.');
  }

  const alreadyUsed =
    Object.values(tournamentConfig.groups)
      .flat()
      .includes(matchId) || tournamentConfig.finalMatchId === matchId;
  if (alreadyUsed) {
    throw new Error('Ese partido ya está asignado en el torneo.');
  }

  const newMatch = await fetchMatchById(matchId);
  const newStatus = normalizeStatus(newMatch.status);
  const newTeams = [
    String(newMatch.homeTeamId),
    String(newMatch.awayTeamId)
  ];

  if (newTeams[0] === newTeams[1]) {
    throw new Error('El partido seleccionado tiene equipos duplicados.');
  }

  const otherMatches = await getAssignedMatchRecords({
    excludeGroup: groupId,
    excludeSlot: slotIndex
  });

  for (const match of otherMatches) {
    const teams = [
      String(match.homeTeamId),
      String(match.awayTeamId)
    ];
    const overlap = teams.some((teamId) => newTeams.includes(teamId));
    if (overlap) {
      throw new Error(
        'Uno de los equipos ya tiene otro partido asignado en el bracket.'
      );
    }
  }

  return newMatch;
}

/* ------------------------------------------------------- *
 * Routes
 * ------------------------------------------------------- */

app.get('/health', (req, res) => {
  res.json({ status: 'OK' });
});

app.get('/api/tournaments', async (req, res) => {
  try {
    const force = req.query.refresh === 'true';
    const state = await ensureState(force);
    res.json([state.summary]);
  } catch (error) {
    console.error('[tournament-service] list error', error?.message || error);
    res.status(502).json({ error: 'No se pudieron obtener los torneos.' });
  }
});

app.get('/api/tournaments/:id', async (req, res) => {
  if (req.params.id !== TOURNAMENT_ID) {
    return res.status(404).json({ error: 'Torneo no encontrado.' });
  }

  try {
    const force = req.query.refresh === 'true';
    const state = await ensureState(force);
    res.json(state.detail);
  } catch (error) {
    console.error('[tournament-service] detail error', error?.message || error);
    res.status(502).json({ error: 'No se pudo cargar el torneo solicitado.' });
  }
});

app.put('/api/tournaments/:id/groups/:groupId/slots/:slotIndex', async (req, res) => {
  if (req.params.id !== TOURNAMENT_ID) {
    return res.status(404).json({ error: 'Torneo no encontrado.' });
  }

  const { groupId } = req.params;
  const slotIndex = Number.parseInt(req.params.slotIndex, 10);

  if (!Object.prototype.hasOwnProperty.call(tournamentConfig.groups, groupId)) {
    return res.status(400).json({ error: 'Grupo inválido.' });
  }

  if (!Number.isInteger(slotIndex) || slotIndex < 0 || slotIndex > 1) {
    return res.status(400).json({ error: 'Posición de slot inválida.' });
  }

  const { matchId } = req.body || {};

  try {
    if (matchId === null || matchId === undefined) {
      tournamentConfig.groups[groupId][slotIndex] = null;
      tournamentConfig.finalMatchId = null;
      resetCache();
      const state = await ensureState(true);
      return res.json(state.detail);
    }

    await validateAssignment(groupId, slotIndex, Number(matchId));
    tournamentConfig.groups[groupId][slotIndex] = Number(matchId);
    tournamentConfig.finalMatchId = null;
    resetCache();
    const state = await ensureState(true);
    return res.json(state.detail);
  } catch (error) {
    console.error(
      '[tournament-service] assignment error',
      error?.message || error
    );
    return res.status(400).json({
      error: error?.message || 'No se pudo asignar el partido al bracket.'
    });
  }
});

app.patch('/api/tournaments/:id/matches/:mid', async (req, res) => {
  if (req.params.id !== TOURNAMENT_ID) {
    return res.status(404).json({ error: 'Torneo no encontrado.' });
  }

  try {
    const matchId = Number(req.params.mid);
    if (!Number.isInteger(matchId) || matchId <= 0) {
      return res.status(400).json({ error: 'Identificador de partido inválido.' });
    }

    const { scoreA = null, scoreB = null, status } = req.body || {};
    if (status === 'finished') {
      await finishMatchInMatchesService(matchId, scoreA, scoreB);
    }

    resetCache();
    const state = await ensureState(true);
    res.json(state.detail);
  } catch (error) {
    console.error('[tournament-service] update error', error?.message || error);
    res.status(502).json({ error: 'No se pudo actualizar el partido.' });
  }
});

app.listen(PORT, () => {
  console.log(`[tournament-service] listening on :${PORT}`);
});
