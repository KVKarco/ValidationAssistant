namespace KVKarco.ValidationAssistant.Tests.Models;

/// <summary>
/// Provides factory methods for creating TestUser instances for testing purposes.
/// </summary>
public static class TestUserDataFactory
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Creates a static, predefined TestUser instance for consistent testing.
    /// </summary>
    /// <returns>A TestUser instance with fixed values.</returns>
    public static TestUser CreateStaticUser()
    {
        return new TestUser
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Age = 30,
            IsActive = true, // Corrected: Property now exists in TestUser
            Status = UserStatus.Active,
            Tags = new List<string> { "Developer", "Tester" },
            Address = new TestAddress
            {
                Street = "123 Main St",
                HouseNumber = 42,
                City = "Anytown",
                State = new StateInfo
                {
                    Name = "California",
                    Abbreviation = "CA",
                    Population = 39000000
                }
            },
            PrimaryContact = new ContactInfo
            {
                PhoneNumber = "555-123-4567",
                Email = "john.doe.contact@example.com",
                UserPreferences = new Preferences
                {
                    ReceiveNotifications = true,
                    PreferredLanguage = "en-US",
                    AppSettings = new Settings
                    {
                        ThemeId = 101,
                        IsDarkMode = true,
                        LastAccessedFeature = "Dashboard"
                    }
                }
            },
            ParentEmail = null
        };
    }

    /// <summary>
    /// Creates a TestUser instance with random data.
    /// </summary>
    /// <param name="includeNulls">If true, some nullable properties might be null.</param>
    /// <returns>A TestUser instance with random values.</returns>
    public static TestUser CreateRandomUser(bool includeNulls = true)
    {
        var user = new TestUser
        {
            Id = _random.Next(1000, 9999),
            Name = $"User{_random.Next(10000)}",
            Email = $"user{_random.Next(10000)}@example.com",
            Age = _random.Next(10, 90),
            IsActive = _random.Next(2) == 0,
            Status = (UserStatus)_random.Next(Enum.GetNames(typeof(UserStatus)).Length),
            Tags = Enumerable.Range(0, _random.Next(0, 5)).Select(i => $"Tag{_random.Next(100)}").ToList(),
            Birthday = includeNulls && _random.Next(2) == 0 ? (DateTime?)null : DateTime.Now.AddDays(-_random.Next(365 * 18, 365 * 60)),
            Salary = (decimal)(_random.NextDouble() * 100000),
            ParentEmail = includeNulls && _random.Next(2) == 0 ? null : $"parent{_random.Next(1000)}@example.com"
        };

        if (!includeNulls || _random.Next(2) == 0) // Roughly 50% chance to be null if includeNulls is true
        {
            user.Address = new TestAddress
            {
                Street = $"Street{_random.Next(100)}",
                HouseNumber = _random.Next(1, 200),
                City = $"City{_random.Next(100)}",
                State = (!includeNulls || _random.Next(2) == 0) ? new StateInfo
                {
                    Name = $"State{_random.Next(10)}",
                    Abbreviation = $"S{_random.Next(10)}",
                    Population = includeNulls && _random.Next(2) == 0 ? (int?)null : _random.Next(1000000, 50000000)
                } : null
            };
        }

        if (!includeNulls || _random.Next(2) == 0) // Roughly 50% chance to be null if includeNulls is true
        {
            user.PrimaryContact = new ContactInfo
            {
                PhoneNumber = $"555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                Email = $"contact{_random.Next(10000)}@example.com",
                UserPreferences = (!includeNulls || _random.Next(2) == 0) ? new Preferences
                {
                    ReceiveNotifications = includeNulls && _random.Next(2) == 0 ? (bool?)null : _random.Next(2) == 0,
                    PreferredLanguage = _random.Next(2) == 0 ? "en-US" : "es-ES",
                    AppSettings = (!includeNulls || _random.Next(2) == 0) ? new Settings
                    {
                        ThemeId = includeNulls && _random.Next(2) == 0 ? (int?)null : _random.Next(1, 5),
                        IsDarkMode = _random.Next(2) == 0,
                        LastAccessedFeature = $"Feature{_random.Next(1, 10)}"
                    } : (Settings?)null // Nullable struct
                } : null
            };
        }

        return user;
    }
}
