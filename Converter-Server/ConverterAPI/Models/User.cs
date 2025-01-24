namespace ConverterAPI.Models;

public record User
{
    public int id { get; set; }
    public string? login { get; set; }
    public string? password { get; set; }
    public bool premium { get; set; }
    public string? salt { get; set; }
}