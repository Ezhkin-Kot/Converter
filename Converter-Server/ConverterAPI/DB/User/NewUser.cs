namespace ConverterAPI.DB.User;

public record NewUser
{
    public string? login { get; set; }
    public string? password { get; set; }
}