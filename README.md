# UPMSF — New College Opening / Seat Enhancement Application

A multi-step affiliation application portal modelled on the Uttar Pradesh State Medical Faculty form.

**Stack:** Blazor WebAssembly (client) · ASP.NET Core Web API (server) · SQL Server (EF Core).

## Solution layout

```
UPMSF.slnx
src/
  UPMSF.Shared    # DTOs, enums, validation rules shared by client + server
  UPMSF.Server    # Web API + EF Core + hosts the WASM client (single deployable)
  UPMSF.Client    # Blazor WebAssembly UI
```

## How it works

1. **Registration** (fresh applicant) → fills *Applicant's Details* → instantly receives a
   **Registration Code** like `AH91000003` (prefix `AH9` + an atomic SQL sequence value).
   Login credentials = **Phone + Registration Code**.
2. **Login** issues a JWT (stored in browser localStorage, so the session survives
   tab-close / reload / power-cut). On return the user lands on the **Dashboard**.
3. **New Application** → each application gets a sub-number `AH91000003-1`, `-2`, …
   - **New Course** → choose *Course Type* (Diploma / Degree / Masters) → dependent
     *Course* dropdown (seeded from the official list incl. *M.Sc. Medical Physics*).
   - **Seat Enhancement** → course hidden; *No. of Seats Applied (Enhancement)* and
     *No. of Existing Seats* are entered at the **top of Form Part-I**.
4. Wizard steps: **Course → Part-I → Part-II → Documents → Payment → Submit**.
   Each step is **saved server-side and then locked** (greyed/disabled) before the next opens.
   Reopening a draft resumes at the exact step (`CurrentStep`).
5. **Documents**: PDF for certificates, ZIP for record/photo bundles (max 25 MB), stored on
   the server filesystem with the path recorded in the DB.
6. **Payment**: mock gateway (Rs 4,00,000 + 18% GST). Replace `Pay()` in
   `ApplicationsController.Steps.cs` with a real gateway later.
7. **Language toggle** (हिंदी / English) switches form *labels*; all entered **values are
   saved in English only** (per the red note on the form).

## Run locally

Prereqs: .NET 8 SDK, SQL Server LocalDB (or any SQL Server).

```bash
# from repo root — the server auto-applies EF migrations on startup
dotnet run --project src/UPMSF.Server
```

Open the printed URL (e.g. `http://localhost:5098`). The server serves the Blazor client,
the API, and Swagger (`/swagger` in Development).

Manual EF commands (optional):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Name> -p src/UPMSF.Server -s src/UPMSF.Server -o Data/Migrations
dotnet ef database update -p src/UPMSF.Server -s src/UPMSF.Server
```

## Configuration (env-var friendly — for Azure / any host)

All settings are overridable via environment variables (double-underscore = section nesting):

| Setting | Env var | Default |
|---|---|---|
| SQL connection | `ConnectionStrings__Default` | LocalDB `UPMSF` |
| JWT signing key (≥32 chars) | `Jwt__Key` | dev placeholder (**set in prod**) |
| JWT issuer/audience/expiry | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryHours` | `UPMSF` / `UPMSF` / 12 |
| Upload folder | `FileStorage__Root` | `App_Data/uploads` |
| Allowed CORS origins (split client) | `Cors__Origins` | any (dev) |

## Deploy

Single self-contained unit — the API project hosts the WASM client.

```bash
dotnet publish src/UPMSF.Server -c Release -o publish
```

Deploy `publish/` to any host. On **Azure App Service**:

1. Create an App Service (Windows or Linux) + Azure SQL Database.
2. App Service → Configuration → set `ConnectionStrings__Default` (Azure SQL),
   `Jwt__Key`, and `FileStorage__Root` (e.g. `D:\home\data\uploads` on Windows or a
   mounted Azure Files share for persistence across restarts/scale-out).
3. Push the `publish/` output (ZIP deploy, GitHub Actions, or container).

Migrations apply automatically on startup. For multi-instance scale-out, point
`FileStorage__Root` at shared storage (Azure Files) so uploads are visible to all instances.

## Concurrency (≈400 users)

- EF Core with `EnableRetryOnFailure` (transient-fault resilience).
- Registration numbers come from a SQL **sequence** (atomic, no contention).
- Per-application `RowVersion` (optimistic concurrency) guards draft edits.
- Stateless JWT auth → horizontally scalable (no server session affinity needed).
