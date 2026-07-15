# Vinyl Scrobbler API 🎧

## Overview
Vinyl Scrobbler is a centralized backend ecosystem designed to bridge physical vinyl record playback with digital tracking on Last.fm. This C# .NET 9 Web API serves as the core engine, receiving scrobble requests from multiple sources—primarily a custom Amazon Alexa Skill ("vinil inteligente") and a planned Web UI—and executing the business logic to authenticate and log tracks via the Last.fm API.

## Architecture Pattern
The project follows a clean **Controller-Service-Repository** architecture to ensure business logic is decoupled from input sources:
* **Controllers:** Act as the entry points (`AlexaController` for voice webhooks, `AuthController` for the authentication flow, and future controllers for the Web UI).
* **Services:** Contain the core business rules (`ScrobbleService` for batch assembly and Last.fm communication, `LastFmAuthService` for encryption and signatures).
* **Repositories (Next Phase):** Will handle data persistence (mapping Alexa users to their respective Last.fm Session Keys).

## Tech Stack
* **Framework:** .NET 9.0 (ASP.NET Core Web API)
* **Language:** C# 13
* **API Documentation:** Swashbuckle (Swagger)
* **External APIs:** Last.fm API v2.0
* **Voice Integration:** Alexa Skills Kit (ASK) Custom Skill JSON Webhooks
* **Development Tools:** .NET CLI, Visual Studio Code, Ngrok

## Getting Started

### Prerequisites
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* Visual Studio Code
* A [Last.fm API Developer Account](https://www.last.fm/api)
* Ngrok installed globally for local tunneling

### Running Locally
1. Clone the repository and navigate to the project root.
2. Restore dependencies:
```bash
dotnet restore

```


3. Run the application:
```bash
dotnet run

```


4. Access the Swagger UI at `http://localhost:<port>` to inspect and test the endpoints.

---

## 🗺️ Development Roadmap & Status

This section tracks the project's progression and documents the technical solutions applied to overcome specific infrastructure and integration challenges.

### Phase 1: Foundation & Project Setup

* [x] **Initialize Repository:** Created a clean .NET 9 Web API using the standard CLI template.
* [x] **Clean Architecture Setup:** Removed boilerplate files (`WeatherForecast`) and established the core domain folders.
* [x] **Swagger Configuration:** Integrated Swashbuckle for interactive API documentation.
* *Note:* Resolved native .NET 9 OpenAPI conflicts by injecting stable third-party packages.


* [x] **Webhook Endpoint:** Implemented the `POST /api/alexa/webhook` endpoint.

### Phase 2: Alexa Voice Interaction Model

* [x] **Invocation Name:** Configured the custom skill with the invocation name `"vinil inteligente"`.
* *Design Decision:* Changed from "toca discos inteligente" to avoid accidental triggers activating native music streaming services (such as Spotify or Amazon Music).


* [x] **Removal of Conflicting Verbs:** Banned the word "tocar" (to play) from Sample Utterances, prioritizing data action verbs like "registrar" (register), "scrobbler", "salvar" (save), and "anotar" (log).
* [x] **Dynamic Parsing in C#:** Replaced strict DTOs with `JsonElement` in `AlexaController`.
* *Note:* This change shields the API against deserialization errors caused by deep structural differences between `LaunchRequest`, `IntentRequest`, and `SessionEndedRequest` payloads sent by Amazon.


* [x] **Flow Guardians:** Implemented quick returns for termination interactions (`SessionEndedRequest`), preventing fatal `INVALID_RESPONSE` errors in the Alexa simulator.
* [x] **Secure Tunneling with Ngrok:** Configured a secure HTTPS local-to-cloud channel.
* *Infrastructure Fix:* Configured the SSL endpoint in the Alexa Console to explicitly accept wildcard certificates (*"My development endpoint is a sub-domain of a domain that has a wildcard certificate..."*), allowing webhook traffic to bypass Amazon's strict TLS handshake layer.



### Phase 3: Last.fm API Core Integration

* [x] **Developer Credentials:** Acquired `API Key` and `Shared Secret` keys and securely stored them in `appsettings.Development.json`.
* [x] **MD5 Cryptographic Signature:** Implemented the `api_sig` generator required by Last.fm for authenticating write requests.
* *Critical Fix (Error 13):* Adjusted the parameter alphabetical sorting method in `LastFmAuthService` to use `StringComparer.Ordinal`. This guarantees strict ASCII ordering of brackets in batch sending arrays (e.g., `album[0]` vs `track[10]`), resolving the signature rejection error.


* [x] **Web Authentication Flow:** Implemented the `/api/auth/login` (redirecting the user to grant access) and `/api/auth/callback` (retrieving and exchanging the temporary token for the permanent Last.fm `Session Key`) routes.
* [x] **Album Scrobbling Engine:** Implemented the `ScrobbleAlbumAsync` method to process entire albums.
* **Metadata Collection:** Queries the `album.getinfo` endpoint to capture the official tracklist and duration of each song.
* **Time Mathematics (Future Projection):** The engine captures the Unix Timestamp of the exact confirmation moment ("Ground Zero") and projects the timestamp of each subsequent track linearly and progressively (`Previous_Timestamp + Song_Duration`).
* **Safety Fallback:** Added a rule that assumes 180 seconds (3 minutes) if a track does not have duration data registered in the Last.fm database, protecting the user's timeline against simultaneous duplicates.
* **Batching:** Groups all tracks into a single structured `POST` payload and dispatches it at once (respecting Last.fm's limit of 50 tracks per batch request).



### Phase 4: Database & User Mapping (Next Step)

* [ ] **EF Core Configuration:** Choose and install a database provider (PostgreSQL/SQL Server) for persistence.
* [ ] **User Entity Modeling:** Create a schema to store encrypted user credentials.
* [ ] **Alexa-to-Last.fm Mapping:** Retrieve the unique user ID sent in the `System.user.userId` node of the Alexa JSON and map it directly to the associated Last.fm `Session Key` in the database, allowing multiple users to use the skill isolatedly and without re-authentication.

### Phase 5: Web UI (React Frontend)

* [ ] **Web Controller:** Create standardized REST endpoints to feed a web dashboard allowing manual vinyl scrobbles.
