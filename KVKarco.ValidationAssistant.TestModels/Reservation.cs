namespace KVKarco.ValidationAssistant.TestModels;

public class Reservation
{
    public Guid Id { get; set; }

    public Table? Table { get; set; }

    public string? CustomerName { get; set; }

    public DateTime ReservationTime { get; set; }

    public int NumberOfPeople { get; set; }
}