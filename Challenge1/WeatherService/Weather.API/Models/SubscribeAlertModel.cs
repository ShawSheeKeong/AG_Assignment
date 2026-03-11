namespace Weather.API.Models;

public class SubscribeAlertModel
{
    public required string Location { get; set; }
    public decimal Threshold { get; set; }
    public required string Email { get; set; }
}