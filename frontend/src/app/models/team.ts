/**
 * Modelo alineado con el microservicio Teams (Spring Boot)
 */
export interface Team {
  id: number;       // Long en backend → number en TS
  name: string;
  coach?: string;
  city?: string;
}

