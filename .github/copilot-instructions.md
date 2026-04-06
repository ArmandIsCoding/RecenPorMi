# Copilot Instructions for RecenPorMi

## Project Overview

**RecenPorMi** ("Pray for me" in Spanish) is a **Blazor Server** web application for sharing and supporting prayer intentions. Users can publish prayer requests (called *peticiones*), optionally upload up to 5 images, mark themselves as having prayed for a request (called *rezos*), and see live updates across all connected clients via SignalR.

The codebase is Spanish-language throughout: variable names, model properties, method names, comments, UI labels, and documentation files are all in Spanish. When generating or modifying code, follow the same naming conventions.

---

## Repository Layout

```
RecenPorMi/                    ← Solution root
├── RecenPorMi/                ← Main ASP.NET Core project
│   ├── Components/
│   │   ├── Pages/             ← Blazor page components (Home, Publicar, DetalleIntencion, …)
│   │   ├── Layout/            ← MainLayout, NavMenu, ReconnectModal
│   │   ├── Shared/            ← Reusable components (PeticionCard)
│   │   └── Account/           ← Identity UI pages (Login, Register, Manage, …)
│   ├── Data/
│   │   ├── Models/            ← EF Core entity classes (Peticion, Rezo, PeticionImagen)
│   │   ├── ApplicationDbContext.cs
│   │   ├── ApplicationUser.cs ← Custom IdentityUser with DisplayName
│   │   └── Migrations/        ← EF Core migrations (5 so far)
│   ├── Services/
│   │   └── PeticionService.cs ← All business logic; registered as Scoped
│   ├── Hubs/
│   │   └── PeticionHub.cs     ← SignalR hub (NotificarNuevaPeticion, NotificarRezoActualizado)
│   ├── wwwroot/
│   │   ├── app.css            ← Custom styles (purple-gradient theme)
│   │   ├── lib/bootstrap/     ← Bootstrap 5 (do NOT replace)
│   │   └── uploads/           ← User-uploaded petition images (gitignored in production)
│   ├── Program.cs             ← DI registrations, middleware, SignalR/Kestrel config
│   ├── appsettings.json       ← Connection string + logging
│   └── RecenPorMi.csproj
├── RecenPorMi.slnx            ← Solution file (.slnx format, VS 2022 17.x+)
└── *.md                       ← Historical implementation notes (useful as reference)
```

---

## Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core Blazor Server | .NET 10.0 |
| Language | C# | 12 |
| ORM | Entity Framework Core | 10.0.3 |
| Database | SQL Server Express | (local dev) |
| Real-time | SignalR | 10.0.3 |
| Auth | ASP.NET Core Identity | 10.0.3 |
| CSS | Bootstrap 5 + custom app.css | — |

---

## How to Build & Run

```bash
# Build
dotnet build

# Run (development)
dotnet run --project RecenPorMi/RecenPorMi.csproj
# → https://localhost:7217  (HTTPS)
# → http://localhost:5296   (HTTP)

# Publish for deployment
dotnet publish -c Release
```

No separate frontend build step — Blazor Server renders on the server.

---

## Database & Migrations

The app uses **SQL Server Express** with the connection string in `appsettings.json`:

```
Server=SERVER\SQLEXPRESS;Database=RecenPorMiDB;User Id=sa;Password=123456;TrustServerCertificate=True;MultipleActiveResultSets=true
```

> ⚠️ The development credentials are hardcoded in `appsettings.json`. In production these must be moved to environment variables or User Secrets.

### Running Migrations

```bash
# Via EF CLI
dotnet ef migrations add <MigrationName> --project RecenPorMi
dotnet ef database update --project RecenPorMi

# Via Package Manager Console (Visual Studio)
Add-Migration <MigrationName>
Update-Database
```

**Always create a new migration** after changing entity models. Never edit existing migration files.

---

## Data Models

### `Peticion` (Prayer Request)
```csharp
Id, UserId (FK), DescripcionBreve (max 200, required),
ContenidoCompleto (optional), FechaPublicacion, ContadorRezos,
PublicarAnonimamente (bool)
// Navigation: Usuario, Rezos, Imagenes
```

### `Rezo` (Prayer/Support)
```csharp
Id, PeticionId (FK), Fecha, IpHash (SHA256 of client IP, max 64)
// Navigation: Peticion
```

### `PeticionImagen` (Image Attachment)
```csharp
Id, PeticionId (FK), NombreArchivo, RutaImagen, Orden, FechaSubida
// Navigation: Peticion
```

### `ApplicationUser` (extends IdentityUser)
```csharp
DisplayName (string, max 50)
```

---

## Service Layer — `PeticionService`

Registered as **Scoped** in DI. All data access goes through this service — components do not use `ApplicationDbContext` directly.

Key methods:
- `ObtenerPeticionesRecientesAsync()` — Returns last 50 petitions (newest first)
- `ObtenerPeticionPorIdAsync(int id)` — Single petition with all includes
- `CrearPeticionAsync(...)` — Creates petition + saves images to `wwwroot/uploads/`
- `RegistrarRezoAsync(int peticionId, string ipHash)` — Adds a rezo; returns `false` if same IP voted within 5 minutes (anti-spam)

When adding new features that require data access, add methods to `PeticionService` and inject the service into the component.

---

## SignalR — `PeticionHub`

**Hub URL**: `/peticionhub`

Hub methods (called server-side after mutations):
- `NotificarNuevaPeticion()` — Broadcasts new petition event to all clients
- `NotificarRezoActualizado(int peticionId, int nuevoContador)` — Broadcasts updated prayer count

Client-side connection is initialized in `Home.razor` and `DetalleIntencion.razor` using `HubConnectionBuilder` with `WithAutomaticReconnect()`.

**Size limits** (set in `Program.cs`):
- SignalR max receive message: **10 MB**
- Kestrel max request body: **50 MB**

Do not reduce these limits — image uploads depend on them.

---

## Authentication & Authorization

- **ASP.NET Core Identity** (cookie-based, no JWT)
- `RequireConfirmedAccount = false` — Email confirmation is disabled
- To protect a Blazor page, add `@attribute [Authorize]` at the top (see `Publicar.razor`)
- Anonymous posting is supported via `PublicarAnonimamente` flag on `Peticion`
- Identity UI is scaffolded under `Components/Account/`

---

## UI Conventions

- **Language**: All labels, messages, and UI text are in **Spanish**
- **Theme**: Purple gradient (`#667eea` → `#764ba2`); do not change theme colors without updating `app.css` globally
- **Icons**: Emoji-based (🙏, ✨, 📝, 📷, etc.) — continue this pattern
- **CSS**: Custom styles live in `wwwroot/app.css`. Bootstrap utility classes are preferred; add custom rules only when Bootstrap doesn't cover the need
- **Naming**: Follow existing Spanish naming — e.g., `peticion`, `rezo`, `publicar`, `rezar`

---

## Adding New Pages

1. Create a `.razor` file under `Components/Pages/`
2. Add `@page "/ruta"` directive at the top
3. Inject services with `@inject`
4. Register any new service in `Program.cs`
5. Add navigation entry in `Components/Layout/NavMenu.razor`

---

## Image Uploads

- Max **5 images** per petition
- Max **5 MB** per image
- Images are saved to `wwwroot/uploads/` with a GUID filename
- `PeticionService.CrearPeticionAsync` handles file writing and `PeticionImagen` record creation
- `Publicar.razor` handles client-side preview and validation before upload

---

## Anti-Spam

Prayer counting is guarded by IP-based deduplication:
- The client IP is hashed with SHA256 before storage (privacy-preserving)
- `RegistrarRezoAsync` checks for a `Rezo` with the same `(PeticionId, IpHash)` within the past 5 minutes
- Returns `false` (and does not increment) if the check fails

> ⚠️ Current IP retrieval uses a hardcoded fallback `"127.0.0.1"`. In production, real client IPs must be retrieved via `IHttpContextAccessor`. See `Home.razor` and `DetalleIntencion.razor` for where this is called.

---

## Known Issues & Workarounds

| Issue | Workaround / Fix |
|---|---|
| SignalR message size exceeded on image upload | Increased `MaximumReceiveMessageSize` to 10 MB and Kestrel `MaxRequestBodySize` to 50 MB in `Program.cs` |
| Login redirect not working after clicking "Publicar" when logged out | Added explicit navigation logic in `Home.razor` using `NavigationManager` before `[Authorize]` guard triggers |
| EF Core migration conflicts after auth schema change | Applied `IdentitySchemaVersions.Version3` and re-scaffolded migrations |
| Email confirmation blocking registration | Set `RequireConfirmedAccount = false` in Identity options |

---

## No Tests Currently

There is no test project in this solution. When writing or modifying logic, validate manually by running the application. If adding tests, use **xUnit** with the **bUnit** library for Blazor component testing.

---

## No CI/CD Currently

The `.github/workflows/` directory is empty. There are no automated pipelines. All builds and deployments are done manually.

---

## Deployment Target

- **Windows IIS** with the ASP.NET Core hosting bundle
- Connection string and secrets must be configured in environment variables or IIS application settings (not in `appsettings.json`) for production

---

## Documentation Files (Historical Reference)

The following markdown files in the repo root document past implementation decisions. They are useful context but should not be modified:

- `INSTRUCCIONES_MIGRACION.md` — DB migration setup guide
- `MIGRACION_AUTENTICACION.md` — Auth migration steps
- `IMPLEMENTACION_CONTENIDO_IMAGENES_COMPLETADO.md` — Image upload feature completion
- `DISPLAYNAME_IMPLEMENTACION.md` — DisplayName feature notes
- `FIX_SIGNALR_LIMITES.md` — SignalR/Kestrel size limit fix
- `FIX_REDIRECCION_LOGIN.md` — Login redirect fix
- `DEBUG_PUBLICACION.md` — Publishing debug notes
- `DISEÑO_AUTH_PAGES.md` — Auth page design notes
- `CAMBIOS_NAVBAR.md` — NavMenu changes

---

## Quick Reference Checklist for Common Tasks

- **Add a new entity field**: Edit model → create EF migration → update service methods → update Razor component UI
- **Add a new page**: Create `.razor` → add `@page` → update `NavMenu.razor` → register any new services
- **Add a SignalR event**: Add hub method in `PeticionHub.cs` → call from service after mutation → subscribe in component with `hubConnection.On<...>()`
- **Change the prayer counter logic**: Modify `RegistrarRezoAsync` in `PeticionService.cs`
- **Style changes**: Edit `wwwroot/app.css`; prefer Bootstrap utilities for layout
