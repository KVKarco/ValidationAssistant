namespace KVKarco.ValidationAssistant.TestModels;

public sealed class Employee
{
    public Guid Id { get; set; }

    public EmployeeInfo? Info { get; set; }

    public EmployeeRole Role { get; set; }
}
