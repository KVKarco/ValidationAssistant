namespace KVKarco.ValidationAssistant.TestModels;

public struct Address
{
    public string? City { get; set; }

    public string? Street { get; set; }

    public int PostalCode { get; set; }

    public int StreetNumber { get; set; }

    public StreetCoordinates? Coordinates { get; set; }
}
