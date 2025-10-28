# Contribución

## Flujo de Git
- Rama por feature: `feat/reports-xxx`
- PRs con: descripción, screenshots (si aplica), checklist
- Protección de `main`: 1+ review, CI verde

## Convenciones
- Commits: Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`)
- Estilo: `black` + `ruff` (hooks `pre-commit`)

## Checklist PR
- [ ] Lint ok
- [ ] Docker build ok
- [ ] Variables .env documentadas
- [ ] Endpoints documentados en README (si aplica)
