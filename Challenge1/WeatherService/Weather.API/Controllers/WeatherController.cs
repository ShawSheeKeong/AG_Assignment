using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Weather.API.Models;
using Weather.Application.Services;
using Weather.Domain.Entities;

namespace Weather.API.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;
    private readonly AlertService _alertService;

    public WeatherController(WeatherService service, AlertService alertService)
    {
        _weatherService = service;
        _alertService = alertService;
    }

    /// <summary>
    /// Gets the current weather for a specified city.
    /// </summary>
    /// <param name="city">The city name (e.g., "Singapore").</param>
    /// <returns>Current weather data for the city.</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent([Required] string city)
    {
        var result = await _weatherService.GetCurrentWeather(city);

        return Ok(result);
    }

    /// <summary>
    /// Gets the weather forecast for 5 days with data every 3 hours by a specified city.
    /// </summary>
    /// <param name="city">The city name (e.g., "Singapore").</param>
    /// <returns>Hourly forecast data.</returns>
    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast([Required] string city)
    {
        var result = await _weatherService.GetForecastData(city);

        return Ok(result);
    }

    /// <summary>
    /// Gets historical weather data for a city within a date range.
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="from">Start date in dd/MM/yyyy format</param>
    /// <param name="to">End date in dd/MM/yyyy format</param>
    [HttpGet("history")]
    public async Task<ActionResult> GetHistorical([Required] string city, string? from = null, string? to = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City is required.");

        var fromDate = ParseDateOrDefault(from, DateTime.MinValue);
        var toDate = ParseDateOrDefault(to, DateTime.UtcNow);

        var history = await _weatherService.GetHistoricalWeather(city, fromDate, toDate);
        return Ok(history);
    }

    private static DateTime ParseDateOrDefault(string? input, DateTime defaultValue) =>
        DateTime.TryParse(input, new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime result) ? result : defaultValue;

    /// <summary>
    /// Exports historical weather data for a city as a CSV file.
    /// </summary>
    /// <param name="city">City name.</param>
    /// <returns>CSV file containing historical weather data.</returns>
    [HttpGet("export")]
    public async Task<IActionResult> ExportData([Required] string city)
    {
        var csv = new StringBuilder();

        var properties = typeof(WeatherObservation).GetProperties();
        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        var result = await _weatherService.GetHistoricalWeather(city, DateTime.MinValue, DateTime.UtcNow);

        foreach (var w in result)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(w);
                return val?.ToString() ?? string.Empty;
            });
            csv.AppendLine(string.Join(",", values));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()),
                    "text/csv",
                    "weather.csv");
    }

    /// <summary>
    /// Subscribes a user to weather temperature alerts for a specific city.
    /// </summary>
    /// <param name="model">
    /// The subscription request containing the user's email address, target city,
    /// and the temperature threshold that will trigger an alert.
    /// </param>
    /// <returns>
    /// Returns a success message if the subscription is created successfully.
    /// </returns>
    [HttpPost("alerts")]
    public async Task<IActionResult> SubscribeAlerts([FromBody] SubscribeAlertModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var subscription = new WeatherAlertSubscription
        {
            Id = Guid.NewGuid(),
            Location = model.Location,
            Threshold = model.Threshold,
            Email = model.Email
        };

        await _alertService.SaveAlert(subscription);

        return Ok(new
        {
            message = "Successfully subscribed to weather temperature alerts.",
            city = model.Location
        });
    }
}