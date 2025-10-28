# app/security.py
import hmac
import os

from fastapi import HTTPException, status
from jose import JWTError, jwt

AUTH_SECRET = os.getenv("AUTH_SECRET", "")
INTERNAL_SECRET = os.getenv("INTERNAL_SECRET", "")


def verify_admin(authorization: str | None, x_internal_secret: str | None):
    # Vía 1: secreto interno desde Nginx
    if (
        INTERNAL_SECRET
        and x_internal_secret
        and hmac.compare_digest(x_internal_secret, INTERNAL_SECRET)
    ):
        return

    # Vía 2 (opcional para curl): JWT propio del RS
    if not authorization:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, detail="Missing Authorization"
        )

    try:
        scheme, token = authorization.split(" ", 1)
        if scheme.lower() != "bearer":
            raise ValueError("bad scheme")
        claims = jwt.decode(token, AUTH_SECRET, algorithms=["HS256"])
        role = (claims.get("role") or "").lower()
        if role != "admin":
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN, detail="Admin required"
            )
    except (ValueError, JWTError):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token"
        )
