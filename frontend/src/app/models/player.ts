export interface Player {
  id: number;
  name: string;
  age: number;
  position?: string | null;
  teamId?: number | null; // camelCase en frontend
}
