namespace ConverterAPI.DB.User;

public record PublicUser
{
    public int id { get; set; }
    public string? login { get; set; }
    public bool premium { get; set; }
};