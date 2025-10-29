# py-microservicios

Suite of basketball microservices (authentication API, teams, players, matches,
tournaments, reports and Angular frontend).

## Prerequisites

- Docker 24+ and Docker Compose v2.
- Optional: Node.js 18+ if you plan to run the frontend outside Docker.
- GitHub OAuth credentials for the authentication service.

## Initial configuration

1. **Environment variables**
   - Copy the root `.env` file and replace the placeholder GitHub credentials:
     ```bash
     cp .env .env.local && edit .env.local
     ```
     Then export `GITHUB_CLIENT_ID` and `GITHUB_CLIENT_SECRET` (or replace the
     values in `.env` before building). The `auth-service` container reads those
     variables directly.
   - `report-service/.env` is optional. If you need custom tokens copy
     `.env.example`, otherwise the service falls back to sensible defaults.

2. **Persistence**
   - Database containers use named volumes (`mssqldata`, `teams_pgdata`,
     `players_data`, `redis_data`). On a fresh machine you do not need to do
     anything. If you ever want to reset the data run:
     ```bash
     docker volume rm py-microservicios_mssqldata \
       py-microservicios_teams_pgdata \
       py-microservicios_players_data \
       py-microservicios_redis_data
     ```

## First run

```bash
docker compose build
docker compose up -d
docker compose logs -f     # optional: wait until everything is healthy
```

`matches-service` applies Entity Framework migrations on startup with retries,
so no manual migration step is required. `teams-service` uses
`spring.jpa.hibernate.ddl-auto=update` and `data.sql` to create the schema and
seed four sample teams in PostgreSQL automatically.

## Key endpoints

| Service             | URL on host                        | Notes                                      |
| ------------------- | ---------------------------------- | ------------------------------------------ |
| Frontend (Angular + Nginx) | http://localhost                 | Full UI (admin panel, control, scoreboard) |
| Auth API            | http://localhost:5000/swagger      | OAuth, login                               |
| Matches API         | http://localhost:5002/swagger      | Schedule and control matches               |
| Teams API           | http://localhost:8082/api/teams    | Team CRUD (includes sample data)           |
| Tournament service  | http://localhost:8083/api/tournaments | Brackets fed from matches-service       |
| Report service      | http://localhost:8084/health       | FastAPI cache + Redis                      |

## Recommended flow after startup

1. Go to `/admin` and schedule matches at `http://localhost/programar`.
2. Assign scheduled matches to each bracket slot on the Tournaments page.
3. Run games from the control panel (`/control/:id`). When you finish a match
   the tournament service is updated automatically.

## Troubleshooting

- If a service cannot reach its database on the first attempt, stop and start
  the stack again (`docker compose down && docker compose up -d`).
- To reset everything use `docker compose down --volumes`.
- Each service exposes a health or swagger endpoint (see the table above).

## Local development (optional)

```bash
cd frontend
npm install
npm run start
```

The Angular dev server proxies API calls to the Docker services (see
`proxy.conf.json`). Keep the Docker stack running so that the APIs are reachable.
