namespace KVKarco.ValidationAssistant.Tests.Models;

public class User
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;

    public Address? Address { get; set; }
}
