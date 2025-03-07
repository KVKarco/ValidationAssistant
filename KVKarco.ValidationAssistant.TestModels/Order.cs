namespace KVKarco.ValidationAssistant.TestModels;

public class Order
{
    public Guid Id { get; set; }

    public Table? Table { get; set; }

    public List<OrderComponent> OrderComponents { get; set; } = new();

    public DateTime OrderTime { get; set; }

    public decimal TotalPrice { get; set; }
}
