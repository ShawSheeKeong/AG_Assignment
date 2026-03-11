using Weather.Domain.Entities;

namespace Weather.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task SaveAsync(WeatherAlertSubscription subscription);
    Task<List<WeatherAlertSubscription>> GetAsync();
}