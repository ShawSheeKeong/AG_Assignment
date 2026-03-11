using Weather.Application.Interfaces;
using Weather.Domain.Entities;

namespace Weather.Application.Services;

public class WeatherService
{
    private readonly IWeatherRepository _repository;
    private readonly IWeatherProvider _weatherProvider;

    public WeatherService(
        IWeatherRepository repository,
        IWeatherProvider weatherProvider)
    {
        _repository = repository;
        _weatherProvider = weatherProvider;
    }

    public async Task<WeatherObservation> GetCurrentWeather(string city)
    {
        WeatherObservation weather = await _weatherProvider.GetCurrentData(city);
        await _repository.SaveAsync(weather);
        return weather;
    }

    public async Task<List<WeatherObservation>> GetForecastData(string city)
    {
        return await _weatherProvider.GetForecastData(city);
    }

    public async Task<List<WeatherObservation>> GetHistoricalWeather(string city, DateTime from, DateTime to)
    {
        return await _repository.GetAsync(city, from, to);
    }
}