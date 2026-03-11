# Weather Service

A .NET 8 REST API that provides current weather, forecasts, and historical weather data powered by the [OpenWeatherMap API](https://openweathermap.org/). Includes a background worker that monitors temperature thresholds and sends email alerts to subscribers via Mailjet.

---

## Features

- **Current weather** — fetch live temperature, humidity, pressure and more for any city
- **5-day forecast** — 3-hourly forecast data from OpenWeatherMap
- **Historical weather** — query past observations stored in the database
- **CSV export** — download all historical data for a city as a `.csv` file
- **Temperature alerts** — subscribe an email address to receive a notification when temperature exceeds a threshold
- **Background alert worker** — runs on a configurable interval, checks all subscriptions and sends emails via Mailjet
- **Polly retry policy** — HTTP calls to OpenWeatherMap automatically retry up to 3 times with a 2-second backoff
- **Structured logging** — Serilog logs to console with context enrichment
- **Swagger UI** — interactive API docs available in development

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- MS SQL Server (free hosting from https://somee.com/)
- [OpenWeatherMap API key](https://home.openweathermap.org/api_keys) (free tier is sufficient)
- [Mailjet account](https://app.mailjet.com/) with API key and secret key

---

## Getting Started

**1. Clone the repository**

```bash
git clone <your-repo-url>
cd Challenge1/WeatherService
```

**2. Configure settings**

Copy `appsettings.json` and fill in your own values (see [Configuration](#configuration) below):

```bash
cp Weather.API/appsettings.json Weather.API/appsettings.Development.json
```

**3. Apply database migrations**

```bash
dotnet ef database update --project Weather.Infrastructure --startup-project Weather.API
```

**4. Run the API**

```bash
dotnet run --project Weather.API
```

The API will be available at `https://localhost:7xxx` and Swagger UI at `https://localhost:7xxx/swagger`.

---

## Configuration

All settings live in `appsettings.json`. Do **not** commit real secrets — use environment variables or a secrets manager in production.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your SQL Server connection string>"
  },
  "OpenWeatherSettings": {
    "Uri": "https://api.openweathermap.org/",
    "ApiKey": "<your OpenWeatherMap API key>"
  },
  "EmailSettings": {
    "FromEmail": "<sender email address>",
    "FromName": "Weather Service",
    "ApiKey": "<Mailjet API key>",
    "SecretKey": "<Mailjet secret key>"
  },
  "SubscriptionJobSettings": {
    "IntervalInHour": 1
  }
}
```

| Setting                                  | Description                                                     |
| ---------------------------------------- | --------------------------------------------------------------- |
| `ConnectionStrings:DefaultConnection`    | SQL Server connection string for the weather database           |
| `OpenWeatherSettings:Uri`                | Base URL for the OpenWeatherMap API                             |
| `OpenWeatherSettings:ApiKey`             | Your OpenWeatherMap API key                                     |
| `EmailSettings:FromEmail`                | The sender address for alert emails                             |
| `EmailSettings:ApiKey`                   | Mailjet public API key                                          |
| `EmailSettings:SecretKey`                | Mailjet secret key                                              |
| `SubscriptionJobSettings:IntervalInHour` | How often (in hours) the background worker checks subscriptions |

---

## Running Tests

The test project uses **xUnit** and **Moq** with no external dependencies — all infrastructure is mocked.

```bash
# Run all tests
dotnet test Challenge1/WeatherService/Weather.Tests

# Run with code coverage
dotnet test Challenge1/WeatherService/Weather.Tests \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults
```

---

## CI/CD

A GitHub Actions workflow at `.github/workflows/build-and-test.yml` runs automatically on every push and pull request to any branch. It:

1. Restores NuGet packages (with caching)
2. Builds the solution in Release configuration
3. Runs all unit tests
4. Publishes test results as a report on the GitHub commit/PR page
5. Uploads the code coverage report as a build artifact

---

## Project Structure

```
Challenge1/WeatherService/
├── Weather.API/
│   ├── Controllers/
│   │   └── WeatherController.cs
│   ├── Models/
│   │   └── SubscribeAlertModel.cs
│   └── Program.cs
├── Weather.Application/
│   ├── Interfaces/
│   │   ├── ISubscriptionRepository.cs
│   │   ├── IWeatherProvider.cs
│   │   └── IWeatherRepository.cs
│   └── Services/
│       ├── AlertService.cs
│       └── WeatherService.cs
├── Weather.Domain/
│   ├── Entities/
│   │   ├── WeatherAlertSubscription.cs
│   │   └── WeatherObservation.cs
│   └── Options/
│       ├── EmailSettings.cs
│       ├── OpenWeatherSettings.cs
│       └── SubscriptionJobSettings.cs
├── Weather.Infrastructure/
│   ├── DbContext/
│   │   └── WeatherDbContext.cs
│   ├── External/
│   │   ├── DTO/
│   │   │   └── OpenWeatherResponse.cs
│   │   └── OpenWeatherClient.cs
│   ├── Migrations/
│   ├── Repositories/
│   │   ├── SubscriptionRepository.cs
│   │   └── WeatherRepository.cs
│   └── Workers/
│       └── TemperatureWorker.cs
├── Weather.Tests/
│   ├── AlertServiceTests.cs
│   ├── WeatherControllerTests.cs
│   └── WeatherServiceTests.cs
└── WeatherService.sln
```
