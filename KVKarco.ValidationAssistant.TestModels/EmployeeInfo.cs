namespace KVKarco.ValidationAssistant.TestModels;

public sealed class EmployeeInfo
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public Address? HomeAddress { get; set; }

    public Address? WorkAddress { get; set; }
}
