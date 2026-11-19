# Debian-based (glibc) Node image: Vite's and TypeScript's native binaries target glibc.
FROM node:22-bookworm-slim AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Static files + /api reverse proxy, so the browser talks to a single origin (no CORS needed).
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
