namespace Weather.Domain.Entities;

public class WeatherObservation
{
    public Guid Id { get; set; }

    public string Location { get; set; } = string.Empty;

    public decimal Temperature { get; set; } = 0;
    public decimal? MaxTemperature { get; set; } = null;
    public decimal? MinTemperature { get; set; } = null;
    public decimal? Pressure { get; set; } = null;
    public decimal? Humidity { get; set; } = null;
    public decimal? SeaLevel { get; set; } = null;
    public decimal? GrndLevel { get; set; } = null;

    public DateTime RecordedAt { get; set; }
}