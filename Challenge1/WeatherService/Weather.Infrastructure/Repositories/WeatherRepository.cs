using Microsoft.EntityFrameworkCore;
using Weather.Application.Interfaces;
using Weather.Domain.Entities;

namespace Weather.Infrastructure.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly WeatherDbContext _context;

    public WeatherRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(WeatherObservation weather)
    {
        _context.WeatherObservations.Add(weather);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WeatherObservation>> GetAsync(string city, DateTime from, DateTime to)
    {
        return await _context.WeatherObservations
            .Where(x => x.Location == city && x.RecordedAt >= from && x.RecordedAt <= to)
            .ToListAsync();
    }
}