namespace ConverterAPI.Models;

public record NewUser
{
    public string? login { get; set; }
    public string? password { get; set; }
}