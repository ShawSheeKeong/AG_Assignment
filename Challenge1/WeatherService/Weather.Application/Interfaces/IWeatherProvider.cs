using Weather.Domain.Entities;

namespace Weather.Application.Interfaces;

public interface IWeatherProvider
{
    Task<WeatherObservation> GetCurrentData(string city);
    Task<List<WeatherObservation>> GetForecastData(string city);
    Task<List<WeatherObservation>> GetHistoricalData(string city, long start, long end);
}