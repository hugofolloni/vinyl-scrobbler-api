# Vinyl Scrobbler API

## Overview

Vinyl Scrobbler is a backend ecosystem engineered to bridge physical vinyl record playback with digital tracking on Last.fm. This C# .NET 9 Web API serves as the central engine, processing scrobble requests from voice interfaces and web applications to authenticate and log track metadata.

## Tech Stack

* **Framework:** .NET 9.0 (ASP.NET Core Web API)
* **Language:** C# 13
* **API Documentation:** Swashbuckle (Swagger)
* **External APIs:** Last.fm API v2.0, Spotify Web API
* **Voice Integration:** Alexa Skills Kit (ASK) Custom Skill JSON Webhooks
* **Development Tools:** .NET CLI, Visual Studio Code

---

## Core Functionalities

### 1. Unified Multi-Platform Access

* **Alexa Integration:** Process voice-activated scrobble requests through the "vinil inteligente" skill, allowing hands-free interaction with your record collection.


* **Web Dashboard:** Fully compatible with the [Vinyl Scrobbler Web UI](https://vinylscrobbler.netlify.app), supporting manual album searches and scrobble triggers via a responsive frontend.



### 2. Intelligent Scrobbling Engine

* **Automated Metadata:** Queries external providers to capture tracklists, durations, and album art automatically, removing the need for manual data entry.


* **Linear Timestamp Projection:** Calculates accurate track timestamps by projecting song durations from a "Ground Zero" playback start time, ensuring your Last.fm timeline remains ordered.


* **Batch Processing:** Consolidates tracks into structured payloads to efficiently log entire albums in a single operation.



### 3. Smart Search & Authentication

* **Adaptive Provider Search:** Seamlessly toggles between Spotify (for popularity-ranked results) and Last.fm search engines based on environment configuration.


* **Secure OAuth Flow:** Manages user authentication handshakes to exchange temporary tokens for permanent Last.fm `Session Keys`, facilitating secure connectivity for both voice and web platforms.

---

## Deployment & Links

* **Source Code:** [GitHub Repository](https://www.google.com/search?q=https://github.com/hugofolloni/vinyl-scrobbler-web)
* **Web Interface:** [Vinyl Scrobbler Dashboard](https://vinylscrobbler.netlify.app)
* **API Documentation:** Interactive Swagger UI available via the production environment host.

---

## Technical Architecture

* **Controller-Service-Repository Pattern:** Decouples input sources from business logic to maintain scalability.


* **Dependency Injection:** Uses `IOptions<T>` and singleton services to manage API configurations and client lifecycles efficiently.


* **OpenAPI Documentation:** Integrated via Swagger to provide real-time testing and interactive documentation of all available endpoints.



---

## Getting Started

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* A [Last.fm API Developer Account](https://www.last.fm/api)
* A [Spotify Developer Dashboard](https://developer.spotify.com/dashboard) application

### Running Locally

1. **Clone the repository** and navigate to the project root.
2. **Restore dependencies**:
```bash
dotnet restore

```


3. **Run the application**:
```bash
dotnet run

```


4. **Swagger UI:** Access `http://localhost:<port>` to inspect and test endpoints