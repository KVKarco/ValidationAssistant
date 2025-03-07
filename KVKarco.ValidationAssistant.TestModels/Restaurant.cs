namespace KVKarco.ValidationAssistant.TestModels;

public class Restaurant
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public Address? Address { get; set; }

    public List<Employee> Employees { get; set; } = new();

    public List<MenuItem> MenuItems { get; set; } = new();

    public List<Order> Orders { get; set; } = new();

    public List<Table> Tables { get; set; } = new();

    public List<Reservation> Reservations { get; set; } = new();
}
