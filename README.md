
# Vinyl Scrobbler API

## Overview

Vinyl Scrobbler is a centralized backend ecosystem designed to bridge physical vinyl record playback with digital tracking on Last.fm. This C# .NET 9 Web API serves as the core engine, receiving scrobble requests from multiple sources—primarily a custom Amazon Alexa Skill ("toca discos inteligente") and a planned Web UI—and executing the business logic to authenticate and log tracks via the Last.fm API.

## Architecture Pattern

The project follows a clean **Controller-Service-Repository** architecture to ensure business logic is decoupled from input sources:

* **Controllers:** Act as the entry points (`AlexaController` for voice webhooks, `WebController` for the frontend).
* **Services:** Contain the core business rules (`ScrobbleService`, `LastFmAuthService`).
* **Repositories:** Handle data persistence (User mapping, Last.fm Session Keys).

## Tech Stack

* **Framework:** .NET 9.0 (ASP.NET Core Web API)
* **Language:** C# 13
* **API Documentation:** Swashbuckle (Swagger)
* **External APIs:** Last.fm API v2.0
* **Voice Integration:** Alexa Skills Kit (ASK) Custom Skill JSON Webhooks
* **Development Tools:** .NET CLI, Visual Studio Code

## Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* Visual Studio Code
* A [Last.fm API Developer Account](https://www.last.fm/api)

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


4. Access the Swagger UI at `http://localhost:<port>` to inspect the available endpoints.

---

## 🗺️ Development Roadmap & To-Do List

This section tracks the project's progression. It is highly descriptive to provide context for AI assistants aiding in the development process.

### Phase 1: Foundation & Project Setup

* [x] **Initialize Repository:** Create the .NET 9 Web API using the standard CLI template.
* [x] **Clean Architecture Setup:** Remove boilerplate (`WeatherForecast`) and establish `Controllers`, `Services`, `Repositories`, and `Models` directories.
* [x] **Swagger Configuration:** Implement Swashbuckle for API exploration.
* *Note:* Resolved native .NET 9 `Microsoft.AspNetCore.OpenApi` conflicts with Swashbuckle by removing the native package and enforcing the standard 3rd-party library.


* [x] **Alexa DTOs (Data Transfer Objects):** Create strictly typed C# models (`AlexaRequest`, `AlexaResponse`) mapping `camelCase` JSON properties to `PascalCase` C# properties to handle the Amazon ASK webhook format.
* [x] **Webhook Endpoint:** Implement `POST /api/alexa/webhook` to receive interactions, logging the raw JSON and returning a valid minimum viable response to keep the Alexa session healthy.

### Phase 2: Alexa Voice Interaction Model

* [x] **Invocation Name:** Set up the custom skill in the Alexa Developer Console with the invocation name `"toca discos inteligente"`.
* [ ] **Define Intents & Slots:** Create the `ScrobblarVinilIntent` in the Alexa Console. Define an `AMAZON.MusicAlbum` or `AMAZON.SearchQuery` slot named `nomeDoVinil`.
* [ ] **Train Model:** Add utterances (e.g., *"fazer scrobble do vinil {nomeDoVinil}"*) and build the voice model in the console.
* [ ] **Parse Intent in C#:** Update the `AlexaController` to inspect `AlexaRequest.Request.Intent.Name` and extract the `nomeDoVinil` slot value from the payload.
* [ ] **Dynamic Voice Feedback:** Update the `AlexaResponse` to reply dynamically (e.g., *"Preparando para scrobblar o disco {nomeDoVinil}."*).

### Phase 3: Last.fm API Core Integration

* [ ] **Last.fm Developer Setup:** Acquire `API Key` and `Shared Secret`.
* [ ] **Environment Variables:** Securely store Last.fm credentials using .NET User Secrets or `appsettings.json` (do not commit to version control).
* [ ] **Last.fm Authentication Flow (Web Auth):**
* Implement a method to generate the authentication URL for the user to approve the app.
* Implement an endpoint to receive the callback `token`.
* Create the MD5 signature logic required by Last.fm.
* Exchange the `token` for a permanent `sk` (Session Key) via `auth.getSession`.


* [ ] **Scrobble Service:** Create `IScrobbleService` and implement the HTTP POST request to the `track.scrobble` endpoint, utilizing the API signature, timestamp, album/track data, and the Session Key.

### Phase 4: Database & User Mapping

* [ ] **Database Setup:** Install Entity Framework Core (EF Core) and configure a provider (PostgreSQL or SQL Server).
* [ ] **User Entity Model:** Create a schema to store users.
* [ ] **Alexa-to-Last.fm Mapping:** - Extract the Amazon `userId` from the `AlexaRequest`.
* Map this Amazon ID to the specific Last.fm Session Key in the database to allow seamless voice scrobbling without re-authentication.



### Phase 5: Exposing to the World & Web UI

* [ ] **Local Tunnelling:** Set up Ngrok to expose the local `.NET` environment to the public web (HTTPS) and link this URL as the endpoint in the Alexa Developer Console.
* [ ] **Web Controller:** Create a standard RESTful `WebController` for the future React frontend to trigger manual scrobbles.
* [ ] **End-to-End Testing:** Perform a full voice command test through an Echo device.

---