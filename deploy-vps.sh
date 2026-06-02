#!/usr/bin/env bash
# UPMSF one-shot VPS deploy (Ubuntu 22.04/24.04, run as root).
#   - removes any host Caddy (we now run Caddy as a container)
#   - installs Docker if missing
#   - opens firewall ports 80/443
#   - builds + starts db + app + caddy
#
# Run from the project directory AFTER you created .env:
#   cd /opt/upmsf && bash deploy-vps.sh
set -euo pipefail

cd "$(dirname "$0")"

echo "==> Checking .env ..."
if [ ! -f .env ]; then
  echo "ERROR: .env not found. Run:  cp .env.example .env  then edit it (DB_PASSWORD, JWT_KEY, DOMAIN, LETSENCRYPT_EMAIL)."
  exit 1
fi
# fail early if placeholders left unchanged
if grep -q "Change_This" .env || grep -q "yourdomain.com" .env; then
  echo "ERROR: .env still has placeholder values. Edit DB_PASSWORD / JWT_KEY / DOMAIN first."
  exit 1
fi

echo "==> Removing host Caddy (now runs as a container) ..."
systemctl stop caddy 2>/dev/null || true
systemctl disable caddy 2>/dev/null || true
apt-get remove -y caddy 2>/dev/null || true

echo "==> Installing Docker (if missing) ..."
if ! command -v docker >/dev/null 2>&1; then
  curl -fsSL https://get.docker.com | sh
fi
docker --version

echo "==> Opening firewall ports 80/443 (if ufw active) ..."
if command -v ufw >/dev/null 2>&1; then
  ufw allow 22/tcp  || true
  ufw allow 80/tcp  || true
  ufw allow 443/tcp || true
fi

echo "==> Building + starting containers ..."
docker compose up -d --build

echo "==> Status:"
docker compose ps

echo
echo "Done. App: db + app + caddy are up."
echo "Open  https://$(grep -E '^DOMAIN=' .env | cut -d= -f2)  (allow ~30s for the HTTPS cert)."
echo "Logs:  docker compose logs -f app    |    docker compose logs -f caddy"
