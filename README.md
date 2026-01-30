<div align="center">

# Stream‑Linkify Backend (WIP)

Convert a track URL from one streaming provider (Spotify / Apple Music / TIDAL / Deezer / SoundCloud) into equivalent track links (and metadata) on the other supported platforms.

</div>

**See The Demo:** [https://stream-linkify.pages.dev/](https://stream-linkify.pages.dev/)

> Status: Work in progress. Public contract and DTO shapes are still volatile; expect breaking changes.

## ✨ What It Does (Current Scope)

- Accepts a single track URL (Spotify, Apple Music, TIDAL, Deezer, SoundCloud) and returns a unified object containing the canonical metadata + cross‑platform URLs when resolvable.
- Normalizes lookups around ISRC (preferred) falling back to provider specific search heuristics.
- Generates/refreshes API access tokens for each provider (Spotify Client Credentials flow, Apple Music developer token (JWT), TIDAL token flow) on demand.
- Exposes a consolidation endpoint plus a few raw provider test endpoints (subject to removal/refactor).
- Provides OpenAPI docs (via Scalar) in Development at `/scalar`.

## 🧱 High Level Architecture

```
Client -> POST /api/UrlConversion/tracks (track URL)
				└─> Provider Input (Spotify, AppleMusic, etc.) parses + fetches base track attributes
							├─> Resolves ISRC & core metadata
							├─> If ISRC missing and platform lookups fail, refetches ISRC from Spotify/Apple Music (fallback chain) then retries failed lookups once
							├─> Queries other providers by ISRC (or fallback search (by track name and primary artist))
							└─> Aggregates TrackReturnDto (original + alt URLs)

		-> POST /api/UrlConversion/albums (album URL)
				└─> Provider Input (Spotify, AppleMusic, etc.) parses + fetches base album attributes
							├─> Resolves UPC & core metadata
							├─> If UPC missing and platform lookups fail, refetches UPC from Spotify/Apple Music (fallback chain) then retries failed lookups once
							├─> Queries other providers by UPC (or fallback search (by album name and primary artist))
							└─> Aggregates AlbumReturnDto (original + alt URLs)
```

Key layers:
- Controllers: HTTP surface (`UrlConversionController`, provider-specific test controllers like `SpotifyController`).
- Services: Provider logic (token retrieval, track search, mapping).
- Resolvers: Platform-specific URL resolution (`IPlatformUrlResolver`) - each platform has its own resolver (Spotify, Apple, TIDAL, Deezer, SoundCloud) that handles cross-platform lookups.
- DTOs: Strongly typed request/response shapes per provider.
- Mappers: Translate provider responses into internal unified model (`TrackReturnDto`).

## 📡 Endpoints (Current)

| Method | Route | Description | Notes |
|--------|-------|-------------|-------|
| GET | `/` | Simple health/info | Returns basic message. |
| POST | `/api/UrlConversion/tracks` | Convert a provider track URL into cross‑platform links | Body: `{ "trackUrl": "..." }` |
| POST | `/api/UrlConversion/albums` | Convert a provider ALBUM URL into cross‑platform links | Body: `{ "albumUrl": "..." }` |

OpenAPI / Scalar UI available in Development: visit `/scalar` (it internally maps the OpenAPI doc produced by `builder.Services.AddOpenApi()` + `app.MapOpenApi()`).

### Example Request

POST `/api/UrlConversion/tracks`

```json
{
	"trackUrl": "https://open.spotify.com/track/43eLl2gwEr0fgbFgS11uOh"
}
```

### Example (Indicative) Response

Tracks:
```json
{
	"songName": "Example Track",
	"artistNames": "[Example Artist, Example Artist 2]",
    "albumName": "Example Album Name",
    "spotify": "https://open.spotify.com/track/...",
	"apple": "https://music.apple.com/us/album/.../track/...",
	"tidal": "https://listen.tidal.com/track/...",
	"deezer": "https://www.deezer.com/track/...",
	"soundcloud": "https://soundcloud.com/..."
}
```

Field names will solidify later; treat above as illustrative only.

## 🛠️ Local Development

Prerequisites:
- .NET 9 SDK (project targets `net9.0` per build output folders)

Clone & run:
```pwsh
git clone <your-fork-or-origin>
cd Stream-Linkify-Backend
dotnet restore
dotnet build
dotnet run --project Stream-Linkify-Backend/Stream-Linkify-Backend.csproj
```

The app will expose HTTPS (Kestrel default). Visit `/scalar` in a browser (Development only) for interactive docs.

### Running Tests
```pwsh
dotnet test
```

Some smoke tests require provider credentials (set through user secrets / environment variables) and may be skipped/fail if secrets are missing.

## 🔐 Secrets & Credentials

No secrets should be committed. Current code looks up values via configuration and environment variables:

Apple Music (Developer Token):
- Config keys expected: `AppleMusicKit:TeamId`, `AppleMusicKit:KeyId`
- Private key: looked up from env var `APPLE_PRIVATE_KEY` (PEM contents) OR fallback file path `Keys/AuthKey_<KeyId>.p8`.
- Access JWT generated via `Services/Apple/AppleTokenService.cs` but can also be manually created and hard coded in secrets/environment variable
- Requires Apple Developer Account, which is a paid service.
- [Read about accessing Apple Music Kit developer credentials here](https://developer.apple.com/documentation/applemusicapi/generating-developer-tokens)

Spotify (Client Credentials):
- Expected config keys: `Spotify:ClientId`, `Spotify:ClientSecret` (names inferred; confirm or adjust as implemented in token service once added).
- Requires Spotify developer Account.
- [Read about accessing Spotify developer credentials here](https://developer.spotify.com/documentation/web-api/concepts/access-token)

TIDAL:
- Expected config keys: `Tidal:ClientId`, `Tidal:ClientSecret`.
- Requires Tidal developer Account.
- [Read about acceessing TIDAL developer credentials here](https://developer.tidal.com/documentation/api-sdk/api-sdk-quick-start)

SoundCloud:
- Excpects config keys: `SoundCloud:ClientId`, `Soundcloud:ClientSecret`
- Requires Soundcloud account (no specific developer account)
- [Read about accessing Soundcloud developer credentials here](https://developers.soundcloud.com/docs/api/guide)

### IMPORTANT: Do not commit Private Keys
The repository currently contains `.p8` Apple private key files under:
- `Stream-Linkify-Backend/Keys/AuthKey_<KeyId>.p8`
- `Stream-Linkify-Backend.Tests/Keys/AuthKey_<KeyId>.p8`

Add/keep `*.p8` in `.gitignore` (already present) and prefer loading via environment variable.

### Using User Secrets (Development)
From the project directory:
```pwsh
dotnet user-secrets init
dotnet user-secrets set "AppleMusicKit:TeamId" "<team_id>"
dotnet user-secrets set "AppleMusicKit:KeyId"  "<key_id>"
dotnet user-secrets set "Spotify:ClientId"     "<client_id>"
dotnet user-secrets set "Spotify:ClientSecret" "<client_secret>"
dotnet user-secrets set "Tidal:ClientId"     "<client_id>"
dotnet user-secrets set "Tidal:ClientSecret" "<client_secret>"
```
Then export the Apple private key as an environment variable (PowerShell):
```pwsh
$env:APPLE_PRIVATE_KEY = (Get-Content path\to\AuthKey_<KeyId>.p8 -Raw)
```
Optionally add to your profile for persistence.

## 🧪 Smoke Tests Overview
Located in `Stream-Linkify-Backend.Tests`:
- `AppleServices_SmokeTests` - validates Apple Music developer token generation + basic track lookups (requires secrets & a test track URL / ISRC).
- `SpotifyServices_SmokeTests` - validates token acquisition and sample track / album retrieval.
- `TidalServices_SmokeTests` - validates token acquisition and sample track / album retrieval.
- `DeezerService_SmokeTests` - validates ample track/akbum retrieval as well as conversion of 'share' links.

## 📦 Docker (Initial Skeleton)

There is a `Dockerfile` present. Typical build (ensure secrets provided via build args or runtime env vars - never bake secrets into layers):
```pwsh
docker build -t stream-linkify-backend .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development stream-linkify-backend
```
Inject runtime secrets using `-e` flags or a Docker secret management solution.

## 🚧 Roadmap (Planned / Ideas)

- Robust error envelope + problem details.
- Replace ad-hoc provider parsing with a pluggable strategy registry.
- Add caching (e.g., MemoryCache / Redis) for tokens + track metadata.
- Add database for existing searched tracks, call tracks from db first.
- Additional providers (YouTube Music - contingent on API feasibility).
- OpenTelemetry instrumentation (traces / metrics) + structured logging enrichment.
- Rate limiting / resiliency policies (Polly) around provider calls.
- CI workflow (build, test, security scan) + container publish.

## 🧭 Contributing

Before filing large PRs, open an issue to discuss approach. While unstable, maintainer may force-push or refactor aggressively.

## 📄 License

License not yet selected (treat as All Rights Reserved until one is added). If you intend to use this beyond evaluation, open an issue to clarify licensing direction.

## ⚖️ Disclaimer

This project is unaffiliated with Spotify, Apple, Tidal, or any other music streaming service provider. Respect each provider's Terms of Service and rate limits. Do not use this code to circumvent platform restrictions.

---

Questions / ideas? Open an issue or start a discussion once the repo enables it.
