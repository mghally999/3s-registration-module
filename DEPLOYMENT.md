# Deployment guide

The app has two halves that host differently:

- **Frontend** (React SPA) → **Vercel** (static build).
- **API + SQL Server + RabbitMQ** → **Railway** (containers). Vercel cannot run the .NET API or the databases.

CORS on the API is already open (`AllowAnyOrigin`), so the Vercel frontend can call the Railway API with no extra configuration.

---

## 1. Backend on Railway

1. Create a new Railway project from this GitHub repo. Railway reads `railway.json` and builds the API from `backend/Dockerfile`.
2. Add a **SQL Server** service. SQL Server 2019 needs ~2 GB RAM, so it requires Railway's Hobby plan (or use **Azure SQL free tier**, which works with zero code change — just point the connection string at it).
3. Add a **RabbitMQ** service (Railway has a template). _Optional for the demo:_ set `Messaging__UseInMemory=true` to skip RabbitMQ entirely — the core create-registration flow does not depend on the broker.
4. Set these variables on the **API** service:

   | Variable | Value |
   | --- | --- |
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ASPNETCORE_URLS` | `http://0.0.0.0:8080` (and expose port 8080) |
   | `ConnectionStrings__RegistrationDatabase` | `Server=<sql-host>,1433;Database=ThreesRegistration;User Id=sa;Password=<password>;TrustServerCertificate=True;Encrypt=False` |
   | `Messaging__UseInMemory` | `false` (or `true` to skip RabbitMQ) |
   | `Messaging__Host` | `<rabbitmq-host>` |
   | `Messaging__Username` | `<rabbitmq-user>` |
   | `Messaging__Password` | `<rabbitmq-password>` |

5. Deploy. The API applies EF Core migrations + seeds the lookups on startup. Verify:
   - `https://<api>.up.railway.app/swagger`
   - `https://<api>.up.railway.app/health` → `Healthy`

## 2. Frontend on Vercel

1. Import this repo into Vercel. Set the **Root Directory** to `frontend` (Vercel auto-detects Vite via `frontend/vercel.json`).
2. Add an environment variable:

   | Variable | Value |
   | --- | --- |
   | `VITE_API_BASE_URL` | `https://<api>.up.railway.app` |

3. Deploy. The SPA calls the Railway API directly using that base URL.

## 3. Links to send

- **GitHub repo**: the source.
- **Live app**: the Vercel URL.
- **API docs**: `https://<api>.up.railway.app/swagger`.
- **Health**: `https://<api>.up.railway.app/health`.

---

## $0 alternative

If you prefer not to pay for SQL hosting: deploy only the **frontend to Vercel**, keep the backend runnable locally (`docker compose up`), and send the repo + README + `AUDIT.md` + the screenshots in `frontend/docs/`. The reviewer can run the full stack in two commands.
