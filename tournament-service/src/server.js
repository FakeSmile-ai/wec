// src/server.js
const express = require('express');
const cors = require('cors');

const app = express();
app.use(express.json({ limit: '1mb' }));
app.use(cors());

/** In-memory demo dataset (suficiente para que la UI renderice) **/
const tournaments = [
  {
    id: 'cup2025',
    name: 'Copa Invierno 2025',
    status: 'live',
    progress: 0.65,
    matchesPlayed: 26,
    totalMatches: 40,
    scheduleLabel: 'Oct 10 – Nov 5'
  },
  {
    id: 'summer2026',
    name: 'Summer Cup 2026',
    status: 'upcoming',
    progress: 0.0,
    matchesPlayed: 0,
    totalMatches: 48,
    scheduleLabel: 'Jun 12 – Jul 20'
  }
];

// ViewModel detallado para la pestaña
const detailsById = {
  cup2025: {
    id: 'cup2025',
    name: 'Copa Invierno 2025',
    heroTitle: 'Copa Invierno 2025',
    description: 'Fase de grupos y eliminatorias con 8 equipos',
    season: '2025',
    location: 'Guatemala City',
    scheduleLabel: 'Oct 10 – Nov 5',
    domain: 'scoreboard.local',
    status: 'live',
    progress: 0.65,
    matchesPlayed: 26,
    totalMatches: 40,
    summary: 'La Copa Invierno reúne a los mejores 8 equipos en formato mixto.',
    teams: [
      { id: 't1', name: 'Leones', shortName: 'LEO', palette: { primary: '#2563eb', secondary: '#60a5fa' }, players: ['A. López','M. Díaz'] },
      { id: 't2', name: 'Tigres', shortName: 'TIG', palette: { primary: '#f59e0b', secondary: '#fde68a' }, players: ['J. Pérez','C. Ramos'] },
      { id: 't3', name: 'Águilas', shortName: 'AGU', palette: { primary: '#10b981', secondary: '#6ee7b7' }, players: ['R. Castillo','L. Mena'] },
      { id: 't4', name: 'Toros',   shortName: 'TOR', palette: { primary: '#ef4444', secondary: '#fecaca' }, players: ['S. Ortiz','P. Cano'] },
      { id: 't5', name: 'Halcones', shortName: 'HAL', palette: { primary: '#7c3aed', secondary: '#c4b5fd' } },
      { id: 't6', name: 'Pumas',    shortName: 'PUM', palette: { primary: '#0ea5e9', secondary: '#93c5fd' } },
      { id: 't7', name: 'Cóndores', shortName: 'CON', palette: { primary: '#14b8a6', secondary: '#5eead4' } },
      { id: 't8', name: 'Lobos',    shortName: 'LOB', palette: { primary: '#f97316', secondary: '#fed7aa' } }
    ],
    groups: [
      {
        name: 'Grupo A',
        matches: [
          {
            id: 'gA1',
            label: 'Fecha 1',
            round: 'group',
            status: 'finished',
            startTime: '2025-10-11T20:00:00Z',
            teams: [
              { id: 't1', name: 'Leones', shortName: 'LEO', score: 2, isPlaceholder: false, detail: null, palette: null },
              { id: 't2', name: 'Tigres', shortName: 'TIG', score: 1, isPlaceholder: false, detail: null, palette: null }
            ]
          },
          {
            id: 'gA2',
            label: 'Fecha 2',
            round: 'group',
            status: 'scheduled',
            startTime: '2025-10-20T18:00:00Z',
            teams: [
              { id: 't3', name: 'Águilas', shortName: 'AGU', score: null, isPlaceholder: false, detail: null, palette: null },
              { id: 't4', name: 'Toros', shortName: 'TOR', score: null, isPlaceholder: false, detail: null, palette: null }
            ]
          }
        ]
      },
      {
        name: 'Grupo B',
        matches: [
          {
            id: 'gB1',
            label: 'Fecha 1',
            round: 'group',
            status: 'finished',
            startTime: '2025-10-12T21:00:00Z',
            teams: [
              { id: 't5', name: 'Halcones', shortName: 'HAL', score: 0, isPlaceholder: false, detail: null, palette: null },
              { id: 't6', name: 'Pumas', shortName: 'PUM', score: 3, isPlaceholder: false, detail: null, palette: null }
            ]
          }
        ]
      }
    ],
    semi: [],
    final: {
      id: 'final1',
      label: 'Final',
      round: 'final',
      status: 'scheduled',
      startTime: '2025-11-05T20:00:00Z',
      teams: [
        { id: 't1', name: 'Leones', shortName: 'LEO', score: null, isPlaceholder: false, detail: null, palette: null },
        { id: 't6', name: 'Pumas', shortName: 'PUM', score: null, isPlaceholder: false, detail: null, palette: null }
      ]
    },
    winner: null
  }
};

/** Rutas **/
app.get('/api/tournaments', (req, res) => {
  res.json(tournaments);
});

app.get('/api/tournaments/:id', (req, res) => {
  const id = req.params.id;
  const detail = detailsById[id];
  if (!detail) return res.status(404).json({ error: 'Not found' });
  res.json(detail);
});

app.patch('/api/tournaments/:id/matches/:mid', (req, res) => {
  const { id, mid } = req.params;
  const { scoreA, scoreB, status } = req.body || {};
  const detail = detailsById[id];
  if (!detail) return res.status(404).json({ error: 'Tournament not found' });

  // localizar el partido en grupos o final
  const allMatches = [];
  (detail.groups || []).forEach(g => (g.matches || []).forEach(m => allMatches.push(m)));
  if (detail.final) allMatches.push(detail.final);

  const match = allMatches.find(m => m.id === mid);
  if (!match) return res.status(404).json({ error: 'Match not found' });

  if (typeof scoreA === 'number') match.teams[0].score = scoreA;
  if (typeof scoreB === 'number') match.teams[1].score = scoreB;
  if (status) match.status = status;

  // contadores simples
  const played = allMatches.filter(m => m.status === 'finished').length;
  detail.matchesPlayed = played;
  detail.progress = detail.totalMatches ? played / detail.totalMatches : 0;

  res.json(match);
});

const port = process.env.PORT || 8082;
app.listen(port, () => {
  console.log(`[tournament-service] listening on :${port}`);
});
