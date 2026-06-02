# Deploying UPMSF to a Hostinger VPS (Docker + custom domain)

This runs the whole app on one VPS with Docker Compose:

| Container | Role |
|-----------|------|
| `app`   | ASP.NET Core API **+** Blazor WebAssembly client (one container) |
| `db`    | SQL Server 2022 |
| `caddy` | Reverse proxy with **automatic HTTPS** (Let's Encrypt) for your domain |

> ⚠️ Hostinger **shared/web hosting cannot run .NET or Docker.** Use a **Hostinger VPS (KVM)**.
> SQL Server needs RAM — pick a plan with **≥ 4 GB (8 GB recommended)**.

---

## 1. Create the VPS
- Hostinger → VPS → KVM 2 (or higher) → **Ubuntu 22.04/24.04**.
- Note the VPS **public IP**.

## 2. Point your domain at the VPS
In your domain's DNS (Hostinger → Domains → DNS / or your registrar):

| Type | Name | Value |
|------|------|-------|
| A    | `@`  | `<VPS_IP>` |
| A    | `www`| `<VPS_IP>` |

Wait a few minutes for DNS to propagate (`ping yourdomain.com` should show the VPS IP).

## 3. Install Docker on the VPS
SSH in (`ssh root@<VPS_IP>`), then:
```bash
curl -fsSL https://get.docker.com | sh
docker --version
```

## 4. Get the project onto the VPS
Either `git clone <your-repo>` or copy it up with `scp`:
```bash
# from your PC (project root):
scp -r . root@<VPS_IP>:/opt/upmsf
```
Then on the VPS:
```bash
cd /opt/upmsf
```

## 5. Configure secrets + domain
Everything lives in `.env` — no need to touch `Caddyfile`.
```bash
cp .env.example .env
nano .env        # set:
                 #   DB_PASSWORD       strong password
                 #   JWT_KEY           long random secret  (openssl rand -base64 48)
                 #   DOMAIN            yourdomain.com  (no http://)
                 #   LETSENCRYPT_EMAIL you@example.com
```
Caddy reads `DOMAIN` + `LETSENCRYPT_EMAIL` from `.env` and serves both `yourdomain.com`
and `www.yourdomain.com` with an auto-renewing HTTPS certificate. No host Caddy install needed.

## 6. Launch 🚀
```bash
docker compose up -d --build
```
- First build takes a few minutes (it compiles the app + Blazor client).
- `app` waits for `db` and auto-applies the database migrations + seeds courses/districts.
- `caddy` automatically obtains an HTTPS certificate for your domain.

Open **https://yourdomain.com** — done.

## Useful commands
```bash
docker compose ps                 # status
docker compose logs -f app        # app logs
docker compose logs -f caddy      # HTTPS / proxy logs
docker compose down               # stop (keeps data in volumes)
docker compose up -d --build      # redeploy after code changes
```

## Data & backups
Persistent data lives in Docker named volumes:
- `mssql-data` → the SQL Server database
- `uploads`    → uploaded PDF/ZIP files
- `caddy-data` → HTTPS certificates

Backup example:
```bash
docker run --rm -v upmsf_mssql-data:/v -v $PWD:/b alpine tar czf /b/db-backup.tgz -C /v .
```

## Updating the app
```bash
cd /opt/upmsf
git pull            # or scp the new code
docker compose up -d --build
```

---

### Alternative (managed DB)
If you'd rather not run SQL Server on the VPS, remove the `db` service and point
`ConnectionStrings__Default` at **Azure SQL** (or any managed SQL Server). Everything else stays the same.
