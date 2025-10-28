#!/usr/bin/env bash
set -euo pipefail

python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

cp -n .env.example .env || true
pre-commit install || true

echo "Entorno listo. Ajusta .env y agrega tu carpeta app/ cuando avances con la lógica."
