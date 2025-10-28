# Reportería/Estadísticas — Entorno (sin lógica)

Starter para montar el **entorno** del servicio de reportería (FastAPI) siguiendo **buenas prácticas**.
> **No incluye lógica de app**, solo estructura y configuración base.

## Estructura
```
report-service-env/
├─ docs/
│  ├─ SECURITY.md
│  ├─ CONTRIBUTING.md
├─ ops/
│  ├─ nginx-reports-location.conf
│  └─ compose.dev.yml
├─ docker/
│  └─ Dockerfile
├─ scripts/
│  └─ bootstrap.sh
├─ .env.example
├─ .editorconfig
├─ .gitignore
├─ .pre-commit-config.yaml
├─ pyproject.toml
├─ requirements.in
├─ requirements.txt
├─ CODEOWNERS
└─ README.md
```

## Pasos (local)
1) Crea repo y copia este starter.
2) `python -m venv .venv && source .venv/bin/activate` (Windows: `.venv\Scripts\activate`)
3) `pip install -r requirements.txt`
4) Copia `.env.example` a `.env` y ajusta valores.
5) (Opcional) `pre-commit install` para activar hooks locales.
6) Para dev con Redis: `docker compose -f ops/compose.dev.yml up -d`

## Variables de entorno
Revisa `.env.example`. No subas `.env` al repo.

## Lint/Format/Test
- `ruff` y `black` configurados en `pyproject.toml`
- Hooks `pre-commit` para formatear y detectar secretos.

## Docker
- `docker/Dockerfile` optimizado básico (no root, slim).
- `ops/compose.dev.yml` (servicio placeholder + Redis opcional).

## NGINX (gateway)
Incluye `ops/nginx-reports-location.conf` para montar las rutas `/api/reports/*` al contenedor.

## Convenciones
- **Ramas**: `feat/`, `fix/`, `chore/`, `docs/`
- **Commits**: Conventional Commits (`feat:`, `fix:`, `docs:`, etc.)
- **PRs**: lint + build en CI (agregarás workflow cuando tengas código).

> Cuando tengas tus endpoints reales y la carpeta `app/`, solo actualiza `docker/Dockerfile` para copiar el código y arrancar uvicorn.
