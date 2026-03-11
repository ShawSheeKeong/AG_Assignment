using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Weather.Application.Interfaces;
using Weather.Domain.Entities;
using Weather.Domain.Options;
using Weather.Infrastructure.External.Dto;

namespace Weather.Infrastructure.External;

public class OpenWeatherClient : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenWeatherClient(HttpClient httpClient, IOptions<OpenWeatherSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.ApiKey;
    }

    public async Task<WeatherObservation> GetCurrentData(string city)
    {
        // ref: https://openweathermap.org/current?collection=current_forecast#name
        var uri = $"/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";

        var response = await _httpClient.GetFromJsonAsync<CurrentWeatherResponse>(uri);

        return response == null
            ? new WeatherObservation()
            : new WeatherObservation
            {
                Id = Guid.NewGuid(),
                Location = city,
                Temperature = response.main.temp,
                Humidity = response.main.humidity,
                MaxTemperature = response.main.temp_max,
                MinTemperature = response.main.temp_min,
                SeaLevel = response.main.sea_level,
                GrndLevel = response.main.grnd_level,
                RecordedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(response.dt)).UtcDateTime
            };
    }

    public async Task<List<WeatherObservation>> GetForecastData(string city)
    {
        // https://openweathermap.org/forecast5?collection=current_forecast
        var uri = $"/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric";

        var response = await _httpClient.GetFromJsonAsync<ForecastResponse>(uri);
        if (response!.cnt < 1) return new();

        return response.list
            .Select(item => new WeatherObservation()
            {
                Id = Guid.NewGuid(),
                Location = city,
                Temperature = item.main.temp,
                Humidity = item.main.humidity,
                MaxTemperature = item.main.temp_max,
                MinTemperature = item.main.temp_min,
                SeaLevel = item.main.sea_level,
                GrndLevel = item.main.grnd_level,
                RecordedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item.dt)).UtcDateTime
            })
            .ToList();
    }

    public async Task<List<WeatherObservation>> GetHistoricalData(string city, long start, long end)
    {
        // https://openweathermap.org/history?collection=historical (Not in the free tier)
        var uri = $"/data/2.5/history/city?q={city}&type=hour&start={start}&end={end}&appid={_apiKey}";

        var response = await _httpClient.GetFromJsonAsync<HistoricalResponse>(uri);
        if (response!.cnt < 1) return new();

        return response.list
            .Select(item => new WeatherObservation()
            {
                Id = Guid.NewGuid(),
                Location = city,
                Temperature = item.main.temp,
                Humidity = item.main.humidity,
                MaxTemperature = item.main.temp_max,
                MinTemperature = item.main.temp_min,
                SeaLevel = item.main.sea_level,
                GrndLevel = item.main.grnd_level,
                RecordedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item.dt)).UtcDateTime
            })
            .ToList();
    }
}