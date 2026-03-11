using Microsoft.EntityFrameworkCore;
using Weather.Domain.Entities;

namespace Weather.Infrastructure;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherObservation> WeatherObservations { get; set; }
    public DbSet<WeatherAlertSubscription> WeatherAlertSubscriptions { get; set; }
}