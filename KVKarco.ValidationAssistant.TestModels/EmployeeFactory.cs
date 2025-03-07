namespace KVKarco.ValidationAssistant.TestModels;
public static class EmployeeFactory
{
    public static Employee CreateNewEmployeeWithOnlyHomeAddress()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Info = new EmployeeInfo
            {
                FirstName = "Michael",
                LastName = "Scott",
                PhoneNumber = "555-123-4567",
                Email = "michael.scott@example.com",
                HomeAddress = new Address
                {
                    City = "Scranton",
                    Street = "Slough Avenue",
                    PostalCode = 18503,
                    StreetNumber = 1725,
                    Coordinates = new StreetCoordinates
                    {
                        Latitude = 41,
                        Longitude = -75
                    }
                }
            },
            Role = EmployeeRole.Manager
        };

        return employee;
    }
}
