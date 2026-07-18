FROM node:22-bookworm-slim AS dependencies

WORKDIR /app

COPY webapp/package.json webapp/package-lock.json ./
RUN npm ci

FROM node:22-bookworm-slim AS build

WORKDIR /app

ARG NEXT_PUBLIC_API_BASE_URL

ENV NEXT_TELEMETRY_DISABLED=1
ENV NODE_OPTIONS=--max-old-space-size=1024

COPY --from=dependencies /app/node_modules ./node_modules
COPY webapp/ ./

RUN npm run build

FROM node:22-bookworm-slim AS runtime

WORKDIR /app

ENV NEXT_TELEMETRY_DISABLED=1
ENV NODE_OPTIONS=--max-old-space-size=160
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

COPY --from=build --chown=node:node /app/.next/standalone ./
COPY --from=build --chown=node:node /app/.next/static ./.next/static

EXPOSE 3000

USER node

CMD ["node", "server.js"]
