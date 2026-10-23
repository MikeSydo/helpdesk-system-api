# HelpDesk System API

HelpDesk System API is a layered ASP.NET Core Web API for support tickets, categories, users, authentication, and ticket processing.

## Technology stack

- C#
- .NET 10 and ASP.NET Core Web API
- PostgreSQL 17
- Entity Framework Core 10
- JWT bearer authentication
- xUnit for testing
- Docker Compose for local PostgreSQL

## Architecture

```text
src/
├── HelpDesk.Domain       # Entities, enums, domain rules, and exceptions
├── HelpDesk.Application  # Use cases, DTOs, interfaces, and services
├── HelpDesk.DataAccess   # EF Core context, repositories, configurations, migrations
└── HelpDesk.API          # Controllers, authentication, authorization, and HTTP handling

tests/
└── HelpDesk.Tests        # Domain, application, data-access, API, and integration tests
```

The API uses PostgreSQL through Entity Framework Core, JWT bearer authentication, password hashing, and opaque hashed refresh tokens.

## Requirements

- .NET 10 SDK
- Docker
- PostgreSQL 17, normally started through Docker Compose

## Running locally

Start PostgreSQL:

```powershell
docker compose up -d postgres
```

Configure the database connection and JWT signing key through User Secrets or environment variables. Do not commit secrets to `appsettings.json`, `.env`, or source control.

```powershell
dotnet user-secrets set "ConnectionStrings:AppDbContext" "<postgres-connection-string>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj

$key = [Convert]::ToBase64String(
  [Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

dotnet user-secrets set "Jwt:SigningKey" $key `
  --project src\HelpDesk.API\HelpDesk.API.csproj
```

Email confirmation and password recovery use SMTP. The application reads these
settings through normal .NET configuration; do not read or commit `.env`.
Before using registration or password recovery, enter your own values manually:

```powershell
dotnet user-secrets set "Email:Smtp:Host" "<smtp-host>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj
dotnet user-secrets set "Email:Smtp:Port" "<smtp-port>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj
dotnet user-secrets set "Email:Smtp:Username" "<smtp-username>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj
dotnet user-secrets set "Email:Smtp:Password" "<your-smtp-password>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj
dotnet user-secrets set "Email:Smtp:FromAddress" "<sender-email>" `
  --project src\HelpDesk.API\HelpDesk.API.csproj
```

The real SMTP values are intentionally not included in this repository.

Apply migrations:

```powershell
dotnet ef database update `
  --project src\HelpDesk.DataAccess\HelpDesk.DataAccess.csproj `
  --startup-project src\HelpDesk.API\HelpDesk.API.csproj
```

In Development, startup also runs an idempotent seed when
`DatabaseInitialization:Seed` is `true` in `appsettings.Development.json`.
It creates the development users, three categories, and three example tickets.
The base configuration sets this option to `false`, so production does not run
the seed.

Run the API:

```powershell
dotnet run --project src\HelpDesk.API\HelpDesk.API.csproj
```

When started locally with the `Development` environment, pending migrations run
and the idempotent seed is enabled.

Run the API and PostgreSQL with Docker Compose:

```powershell
Copy-Item .env.example .env
# Set JWT_SIGNING_KEY in .env to a value with at least 32 characters.
docker compose up --build
```

The containerized API listens on `http://localhost:8080`. Docker Compose uses the
`Production` environment, so migrations run automatically but development seed
data is not created.

Swagger is available at `/swagger` in Development.

## JWT configuration

```json
{
  "Jwt": {
    "Issuer": "HelpDesk.Api",
    "Audience": "HelpDesk.Client",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30
  }
}
```

`Jwt:SigningKey` is required, must contain at least 32 characters, and must be supplied through User Secrets or an environment variable. Access tokens are short-lived JWTs. Refresh tokens are cryptographically random opaque values; only their SHA-256 hashes are stored.

Refresh-token rotation revokes the previous token and links it to its replacement. Reuse of a revoked token revokes the entire token family.

## API documentation

Executable HTTP examples for all currently implemented API operations are maintained in:

[`src/HelpDesk.API/HelpDesk.API.http`](src/HelpDesk.API/HelpDesk.API.http)

The request collection covers registration, login, refresh, logout, soft account deletion, tickets, ticket processing, categories, and administrative user management.

Registration returns a pending response and does not issue JWTs until the
email is confirmed. Authentication endpoints for this flow are:

| Endpoint | Purpose |
| --- | --- |
| `POST /api/auth/confirm-email` | Consume the confirmation token |
| `POST /api/auth/resend-confirmation` | Resend confirmation without revealing account existence |
| `POST /api/auth/change-password` | Change password for the current authenticated user |
| `POST /api/auth/forgot-password` | Request a password reset email |
| `POST /api/auth/reset-password` | Consume a reset token and set a new password |

Login and refresh reject unconfirmed users. Password changes and password
resets revoke all refresh tokens for the user.

## Authorization

Send access tokens using:

```http
Authorization: Bearer <access-token>
```

Available roles:

| Role | Permissions |
| --- | --- |
| `User` | Create tickets and access or modify owned tickets |
| `Support` | Process tickets and access ticket data |
| `Admin` | Manage users, categories, and ticket processing |

The `AdminOnly` policy protects user management and category mutations. The `SupportOrAdmin` policy protects ticket status and assignment operations.

User identity is read from JWT claims. Clients must not send `currentUserId` in query parameters or request bodies. Only active users with the `Support` role can be assigned to tickets.

Account deletion is a soft delete: the user is deactivated, all refresh tokens are revoked, and historical tickets and comments remain available for auditing.

## Error responses

Errors are returned as Problem Details:

| Status | Meaning |
| --- | --- |
| `400` | Invalid request or domain input |
| `401` | Missing or invalid authentication |
| `403` | Authenticated user lacks permission |
| `404` | Resource does not exist |
| `409` | Resource conflict, such as a duplicate email |
| `500` | Unexpected server error |

## Testing

Run the full test suite:

```powershell
dotnet test tests\HelpDesk.Tests\HelpDesk.Tests.csproj
```

Database-backed tests require a healthy PostgreSQL container:

```powershell
docker compose up -d
```

Integration tests use a test authentication handler and do not require real JWT credentials.
