# Seguridad

- Tokens y secretos **nunca** en el repo. Usar `.env` local y **secrets** en CI/CD.
- Tokens con expiración corta y rotación periódica.
- Rol `admin` requerido para endpoints de reportería.
- CORS mínimo (solo dominios del panel admin).
- Contenedor no-root, imágenes slim y actualizadas.
- Revisar dependencias con `pip list --outdated` y actualizarlas periódicamente.
