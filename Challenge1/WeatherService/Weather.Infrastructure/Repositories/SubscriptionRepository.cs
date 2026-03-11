using Microsoft.EntityFrameworkCore;
using Weather.Application.Interfaces;
using Weather.Domain.Entities;

namespace Weather.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly WeatherDbContext _context;

    public SubscriptionRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(WeatherAlertSubscription subscription)
    {
        _context.WeatherAlertSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WeatherAlertSubscription>> GetAsync()
    {
        return await _context.WeatherAlertSubscriptions.ToListAsync();
    }
}