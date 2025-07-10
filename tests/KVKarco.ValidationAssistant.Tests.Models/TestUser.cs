namespace KVKarco.ValidationAssistant.Tests.Models;

public class TestUser
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public TestAddress? Address { get; set; }
    public DateTime? Birthday { get; set; }
    public decimal Salary { get; set; }
    public ContactInfo? PrimaryContact { get; set; }
    public UserStatus Status { get; set; }
    public List<string>? Tags { get; set; }
    public string? ParentEmail { get; set; }
    public bool IsActive { get; set; }
}
