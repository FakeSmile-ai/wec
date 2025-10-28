FROM node:20-alpine
WORKDIR /app

# Copia manifest
COPY package*.json ./

# Si hay lockfile -> npm ci; si no -> npm install
RUN set -eux; \
    if [ -f package-lock.json ] || [ -f npm-shrinkwrap.json ]; then \
      npm ci --omit=dev; \
    else \
      npm install --omit=dev; \
    fi

# Copia el resto
COPY . .

ENV NODE_ENV=production
ENV PORT=8082
EXPOSE 8082

# Arranque flexible
CMD [ "sh", "-c", "if npm run | grep -q ' start'; then npm start; elif [ -f src/server.js ]; then node src/server.js; elif [ -f server.js ]; then node server.js; else node index.js; fi" ]
