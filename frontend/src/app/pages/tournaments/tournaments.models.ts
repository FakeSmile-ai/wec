export type TournamentRound = 'group' | 'semi' | 'final';

export interface TournamentTeamStat {
  label: string;
  value: string;
}

export interface TournamentTeamDetail {
  id: string;
  name: string;
  shortName?: string;
  city?: string;
  coach?: string;
  seed?: number;
  record?: string;
  ranking?: number;
  streak?: string;
  palette: { primary: string; secondary: string };
  narrative: string;
  players: string[];
  stats: TournamentTeamStat[];
}

export interface TournamentMatchLink {
  matchId: string;
  slot: 'teamA' | 'teamB';
}

export interface TournamentMatchState {
  id: string;
  label: string;
  round: TournamentRound;
  status: 'scheduled' | 'live' | 'finished';
  scheduledAtUtc?: string | null;
  venue?: string | null;
  groupId?: string;
  broadcast?: string | null;
  teamAId: string | null;
  teamBId: string | null;
  teamAOrigin?: string | null;
  teamBOrigin?: string | null;
  scoreA: number | null;
  scoreB: number | null;
  next?: TournamentMatchLink[];
}

export interface TournamentGroupState {
  id: string;
  name: string;
  color?: string;
  initialMatchIds: string[];
  semiFinalMatchId: string;
}

export interface TournamentState {
  id: string;
  code: string;
  name: string;
  heroTitle: string;
  season: string;
  location: string;
  venue: string;
  startDateUtc: string;
  endDateUtc: string;
  updatedAtUtc: string;
  description: string;
  domain: string;
  teams: TournamentTeamDetail[];
  matches: TournamentMatchState[];
  groups: TournamentGroupState[];
  finalMatchId: string;
}

export interface TournamentMatchTeamSlot {
  id: string | null;
  displayName: string;
  shortName?: string;
  seed?: number;
  record?: string;
  score: number | null;
  isPlaceholder: boolean;
  originLabel?: string | null;
  detail: TournamentTeamDetail | null;
  palette: { primary: string; secondary: string } | null;
}

export interface TournamentMatchView {
  id: string;
  label: string;
  round: TournamentRound;
  status: 'scheduled' | 'live' | 'finished';
  statusLabel: string;
  scheduleLabel: string;
  scheduledAtUtc?: string | null;
  venue?: string | null;
  broadcast?: string | null;
  teamA: TournamentMatchTeamSlot;
  teamB: TournamentMatchTeamSlot;
  winnerId: string | null;
}

export interface TournamentGroupView {
  id: string;
  name: string;
  color?: string;
  matches: TournamentMatchView[];
  qualifiers: TournamentMatchTeamSlot[];
  semiFinal: TournamentMatchView | null;
}

export interface TournamentSummary {
  id: string;
  code: string;
  name: string;
  season: string;
  heroTitle: string;
  location: string;
  scheduleLabel: string;
  progress: number;
  matchesPlayed: number;
  totalMatches: number;
}

export interface TournamentViewModel {
  id: string;
  code: string;
  name: string;
  heroTitle: string;
  season: string;
  location: string;
  venue: string;
  description: string;
  scheduleLabel: string;
  updatedLabel: string;
  domain: string;
  progress: number;
  matchesPlayed: number;
  totalMatches: number;
  summary: string;
  groups: TournamentGroupView[];
  final: TournamentMatchView | null;
  winner: TournamentMatchTeamSlot | null;
  teams: TournamentTeamDetail[];
  teamsIndex: Record<string, TournamentTeamDetail>;
}

export interface UpdateMatchRequest {
  scoreA: number;
  scoreB: number;
  status?: 'scheduled' | 'live' | 'finished';
}
