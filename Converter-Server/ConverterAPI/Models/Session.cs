namespace ConverterAPI.Models;

public record Session()
{
    public int userid { get; set; }
    public DateTime datetime { get; set; }
    public int amount { get; set; }
    public bool active { get; set; }
}