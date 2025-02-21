namespace ConverterAPI.Models;

public record UpdatedUser
{
    public string? currentLogin { get; set; }
    public string? currentPassword { get; set; }
    public string? newLogin { get; set; }
    public string? newPassword { get; set; }
    public bool premium { get; set; }
}