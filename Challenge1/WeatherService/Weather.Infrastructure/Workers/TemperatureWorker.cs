using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Weather.Application.Interfaces;
using Weather.Domain.Options;

namespace Weather.Infrastructure.Workers;

public class TemperatureWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TemperatureWorker> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly SubscriptionJobSettings _subJobSettings;

    public TemperatureWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<TemperatureWorker> logger,
        IOptions<EmailSettings> emailOption,
        IOptions<SubscriptionJobSettings> subJobOption)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _emailSettings = emailOption.Value;
        _subJobSettings = subJobOption.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TemperatureWorker started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var subscriptionRepo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
                var weatherProvider = scope.ServiceProvider.GetRequiredService<IWeatherProvider>();

                var subscriptions = await subscriptionRepo.GetAsync();

                if (!subscriptions.Any())
                {
                    _logger.LogInformation("No subscriptions found at {Time}", DateTime.UtcNow);
                }
                else
                {
                    _logger.LogInformation("Processing {Count} subscriptions at {Time}", subscriptions.Count, DateTime.UtcNow);

                    var locations = subscriptions.Select(x => x.Location).Distinct();

                    foreach (var location in locations)
                    {
                        var tempData = await weatherProvider.GetCurrentData(location);

                        if (tempData == null)
                        {
                            _logger.LogWarning("No temperature data retrieved for {Location} at {Time}", location, DateTime.UtcNow);
                            continue;
                        }

                        _logger.LogInformation(
                            "Current temperature in {Location}: {Temp}°C at {Time}",
                            location, tempData.Temperature, DateTime.UtcNow
                        );

                        var triggeredSubs = subscriptions
                            .Where(x => x.Location == location && tempData.Temperature >= x.Threshold)
                            .ToList();

                        foreach (var sub in triggeredSubs)
                        {
                            var subject = $"Temperature Alert for {sub.Location}";
                            var body = $"Current temperature in {sub.Location} is {tempData.Temperature}°C, exceeding your threshold of {sub.Threshold}°C.";

                            await SendEmailAsync(sub.Email, subject, body);
                            _logger.LogInformation(
                                "Alert sent to {Email} for {Location}, threshold {Threshold}°C exceeded at {Time}",
                                sub.Email, sub.Location, sub.Threshold, DateTime.UtcNow
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing temperature alerts at {Time}", DateTime.UtcNow);
            }
            await Task.Delay(TimeSpan.FromHours(_subJobSettings.IntervalInHour), stoppingToken);
        }

        _logger.LogInformation("TemperatureWorker stopping at {Time}", DateTime.UtcNow);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var client = new MailjetClient(_emailSettings.ApiKey, _emailSettings.SecretKey);

        var request = new MailjetRequest
        {
            Resource = Send.Resource,
        }
        .Property(Send.FromEmail, _emailSettings.FromEmail)
        .Property(Send.FromName, _emailSettings.FromName)
        .Property(Send.Subject, subject)
        .Property(Send.TextPart, body)
        .Property(Send.Recipients, new JArray {
            new JObject { { "Email", toEmail } }
        });

        try
        {
            var response = await client.PostAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Mailjet failed: {response.StatusCode} - {response.GetErrorMessage()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email via Mailjet: {ex.Message}");
        }
    }
}
