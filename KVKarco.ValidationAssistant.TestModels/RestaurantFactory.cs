namespace KVKarco.ValidationAssistant.TestModels;
using System;
using System.Collections.Generic;


public static class RestaurantFactory
{
    public static Restaurant CreateNewRestaurantWithDetails()
    {
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "The Gourmet Kitchen",
            Address = new Address
            {
                City = "New York",
                Street = "5th Avenue",
                PostalCode = 10001,
                StreetNumber = 123,
                Coordinates = new StreetCoordinates
                {
                    Latitude = 40,
                    Longitude = -74
                }
            },
            Employees = new List<Employee>
                {
                    new Employee
                    {
                        Id = Guid.NewGuid(),
                        Info = new EmployeeInfo
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            PhoneNumber = "123-456-7890",
                            Email = "john.doe@example.com",
                            HomeAddress = new Address
                            {
                                City = "New York",
                                Street = "Broadway",
                                PostalCode = 10002,
                                StreetNumber = 456,
                                Coordinates = new StreetCoordinates
                                {
                                    Latitude = 40,
                                    Longitude = -74
                                }
                            }
                        },
                        Role = EmployeeRole.Cook
                    },
                    new Employee
                    {
                        Id = Guid.NewGuid(),
                        Info = new EmployeeInfo
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            PhoneNumber = "987-654-3210",
                            Email = "jane.smith@example.com",
                            HomeAddress = new Address
                            {
                                City = "New York",
                                Street = "Wall Street",
                                PostalCode = 10005,
                                StreetNumber = 789,
                                Coordinates = new StreetCoordinates
                                {
                                    Latitude = 40,
                                    Longitude = -74
                                }
                            }
                        },
                        Role = EmployeeRole.Waiter
                    },
                    new Employee
                    {
                        Id = Guid.NewGuid(),
                        Info = new EmployeeInfo
                        {
                            FirstName = "Alice",
                            LastName = "Johnson",
                            PhoneNumber = "555-123-4567",
                            Email = "alice.johnson@example.com",
                            HomeAddress = new Address
                            {
                                City = "New York",
                                Street = "Madison Avenue",
                                PostalCode = 10022,
                                StreetNumber = 101,
                                Coordinates = new StreetCoordinates
                                {
                                    Latitude = 40,
                                    Longitude = -74
                                }
                            }
                        },
                        Role = EmployeeRole.Manager
                    },
                    new Employee
                    {
                        Id = Guid.NewGuid(),
                        Info = new EmployeeInfo
                        {
                            FirstName = "Bob",
                            LastName = "Brown",
                            PhoneNumber = "444-555-6666",
                            Email = "bob.brown@example.com",
                            HomeAddress = new Address
                            {
                                City = "New York",
                                Street = "Lexington Avenue",
                                PostalCode = 10016,
                                StreetNumber = 202,
                                Coordinates = new StreetCoordinates
                                {
                                    Latitude = 40,
                                    Longitude = -74
                                }
                            }
                        },
                        Role = EmployeeRole.Waiter
                    }
                },
            Tables = new List<Table>
                {
                    new Table
                    {
                        Id = Guid.NewGuid(),
                        Number = 1,
                        Capacity = 4
                    },
                    new Table
                    {
                        Id = Guid.NewGuid(),
                        Number = 2,
                        Capacity = 4
                    },
                    new Table
                    {
                        Id = Guid.NewGuid(),
                        Number = 3,
                        Capacity = 6
                    }
                },
            Reservations = new List<Reservation>
                {
                    new Reservation
                    {
                        Id = Guid.NewGuid(),
                        CustomerName = "Charlie Brown",
                        ReservationTime = DateTime.Now.AddHours(1),
                        NumberOfPeople = 3,
                        Table = new Table
                        {
                            Id = Guid.NewGuid(),
                            Number = 1,
                            Capacity = 4
                        }
                    },
                    new Reservation
                    {
                        Id = Guid.NewGuid(),
                        CustomerName = "Diana Prince",
                        ReservationTime = DateTime.Now.AddHours(2),
                        NumberOfPeople = 5,
                        Table = new Table
                        {
                            Id = Guid.NewGuid(),
                            Number = 2,
                            Capacity = 4
                        }
                    },
                    new Reservation
                    {
                        Id = Guid.NewGuid(),
                        CustomerName = "Eve Adams",
                        ReservationTime = DateTime.Now.AddHours(3),
                        NumberOfPeople = 2,
                        Table = new Table
                        {
                            Id = Guid.NewGuid(),
                            Number = 3,
                            Capacity = 6
                        }
                    }
                }
        };

        return restaurant;
    }
}


