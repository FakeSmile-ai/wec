import os
from dataclasses import dataclass

from redis import asyncio as aioredis


@dataclass
class Cache:
    redis: aioredis.Redis
    prefix: str = "cache:"
    ttl_seconds: int = 300  # 5 minutos

    @classmethod
    def from_env(cls) -> "Cache":
        url = os.getenv("REDIS_URL", "redis://redis:6379/0")
        ttl = int(os.getenv("CACHE_TTL_SECONDS", "300"))
        return cls(
            redis=aioredis.from_url(url, decode_responses=False), ttl_seconds=ttl
        )

    async def get(self, key: str) -> bytes | None:
        v = await self.redis.get(self.prefix + key)
        return v

    async def set(self, key: str, value: bytes) -> None:
        await self.redis.set(self.prefix + key, value, ex=self.ttl_seconds)
