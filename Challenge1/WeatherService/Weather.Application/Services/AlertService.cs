using Weather.Application.Interfaces;
using Weather.Domain.Entities;

namespace Weather.Application.Services;

public class AlertService
{
    private readonly ISubscriptionRepository _repository;

    public AlertService(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task SaveAlert(WeatherAlertSubscription subscription)
    {
        await _repository.SaveAsync(subscription);
    }

    public async Task<List<WeatherAlertSubscription>> GetAlert()
    {
        return await _repository.GetAsync();
    }
}