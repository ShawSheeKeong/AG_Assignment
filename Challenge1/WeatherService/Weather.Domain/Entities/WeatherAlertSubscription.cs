namespace Weather.Domain.Entities;

public class WeatherAlertSubscription
{
    public Guid Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Threshold { get; set; } = 0;
    public string Email { get; set; } = string.Empty;
}