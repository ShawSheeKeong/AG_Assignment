namespace Weather.Domain.Options;

public class OpenWeatherSettings
{
    public string Uri { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}