#!/bin/bash
set -e

# Espera “real” a que acepte conexiones SQL, con reintentos
echo "Esperando a SQL Server en db:1433 ..."
for i in {1..60}; do
  /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1 && break
  echo "  intento $i/60..."; sleep 2
done

# Ejecuta tu script de init (crear DB, login usuario app, etc.)
echo "Ejecutando init.sql ..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$MSSQL_SA_PASSWORD" -i /docker-entrypoint-initdb.d/init.sql
echo "init.sql OK"
