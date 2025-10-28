FROM python:3.12-slim AS base

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

# Dependencias mínimas del sistema
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
  && rm -rf /var/lib/apt/lists/*

# Dependencias Python
COPY requirements.txt /app/requirements.txt
RUN pip install -r requirements.txt

# Copia de la app y ejemplo de .env
COPY app /app/app
COPY .env.example /app/.env.example

# Usuario no-root
RUN useradd -u 10001 -ms /bin/bash appuser
USER appuser

EXPOSE 8080

# Arranque del API (sin reload; es prod-like)
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8080"]
