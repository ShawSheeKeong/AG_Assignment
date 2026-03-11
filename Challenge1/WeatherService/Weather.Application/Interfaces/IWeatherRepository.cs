using Weather.Domain.Entities;

namespace Weather.Application.Interfaces;

public interface IWeatherRepository
{
    Task SaveAsync(WeatherObservation weather);
    Task<List<WeatherObservation>> GetAsync(string city, DateTime from, DateTime to);
}