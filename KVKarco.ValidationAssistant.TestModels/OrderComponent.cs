namespace KVKarco.ValidationAssistant.TestModels;

public class OrderComponent
{
    public Guid Id { get; set; }

    public MenuItem? MenuItem { get; set; }

    public int Quantity { get; set; }
}
