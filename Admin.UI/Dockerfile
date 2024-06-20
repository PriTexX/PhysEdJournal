# This Dockerfile uses `serve` npm package to serve the static files with node process.
# You can find the Dockerfile for nginx in the following link:
# https://github.com/refinedev/dockerfiles/blob/main/vite/Dockerfile.nginx
FROM refinedev/node:18 AS base

ARG NODE_ENV=production

FROM base as deps

COPY package.json yarn.lock* package-lock.json* pnpm-lock.yaml* .npmrc* ./

RUN \
  if [ -f yarn.lock ]; then yarn --frozen-lockfile; \
  elif [ -f package-lock.json ]; then npm ci; \
  elif [ -f pnpm-lock.yaml ]; then yarn global add pnpm && pnpm i --frozen-lockfile; \
  else echo "Lockfile not found." && exit 1; \
  fi

FROM base as builder

ENV NODE_ENV=$NODE_ENV

COPY --from=deps /app/refine/node_modules ./node_modules

COPY . .

RUN npm run build

FROM base as runner

ENV NODE_ENV=$NODE_ENV

RUN npm install -g serve

COPY --from=builder /app/refine/dist ./physedjournal/admin/

USER refine

CMD ["serve"]
