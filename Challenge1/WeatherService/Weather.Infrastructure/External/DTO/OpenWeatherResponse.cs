namespace Weather.Infrastructure.External.Dto;

public class WeatherMain
{
    public decimal temp { get; set; }
    public decimal? humidity { get; set; }
    public decimal? temp_min { get; set; }
    public decimal? temp_max { get; set; }
    public decimal? sea_level { get; set; }
    public decimal? grnd_level { get; set; }
}

public class WeatherItem
{
    public WeatherMain main { get; set; } = new WeatherMain();
    public long dt { get; set; }
}

public class CurrentWeatherResponse
{
    public WeatherMain main { get; set; } = new WeatherMain();
    public long dt { get; set; }
}

public class ForecastResponse
{
    public int cnt { get; set; }
    public List<WeatherItem> list { get; set; } = new();
}

public class HistoricalResponse
{
    public int cnt { get; set; }
    public List<WeatherItem> list { get; set; } = new();
}