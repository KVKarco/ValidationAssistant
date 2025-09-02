using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Tests.Models;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressionFactoryTests
{
    [Fact]
    public void CreateCtx_IdentityExpression_ReturnsUndefined_WithValue_AndCorrectPropertyKey()
    {
        // Arrange
        var user = new User();
        Expression<Func<User, User>> expr = x => x;
        var ctx = ExpressionFactory.CreateCtx(expr);

        // Act
        var result = ctx.ExtractValue(user);

        // Assert
        Assert.True(result.IsDefined);
        Assert.Null(result.MissingMember);
        Assert.Equal(user, result.Value);
        Assert.False(result.IsNull);
        Assert.Equal("", ctx.Key.PropertyPath);
        Assert.Equal("", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_RootNull_ReturnsUndefinedWithNoValue_MainInstance_AndCorrectPropertyKey()
    {
        Expression<Func<User, string?>> expr = x => x.FirstName;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(null!);

        Assert.False(result.IsDefined);
        Assert.Equal("MainInstance", result.MissingMember);
        Assert.True(result.IsNull);
        Assert.Equal("FirstName", ctx.Key.PropertyPath);
        Assert.Equal("FirstName", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_IntermediateClassNull_ReturnsUndefinedWithNoValue_PropertyName_AndCorrectPropertyKey()
    {
        var user = new User { Address = null };
        Expression<Func<User, string?>> expr = x => x.Address!.Street;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(user);

        Assert.False(result.IsDefined);
        Assert.Equal("Address", result.MissingMember);
        Assert.True(result.IsNull);
        Assert.Equal("Address.Street", ctx.Key.PropertyPath);
        Assert.Equal("Street", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_IntermediateNullableStructNull_ReturnsUndefinedWithNoValue_PropertyName_AndCorrectPropertyKey()
    {
        var user = new User
        {
            Address = new Address { Coordinates = null }
        };
        Expression<Func<User, double>> expr = x => x.Address!.Coordinates!.Value.Latitude;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(user);

        Assert.False(result.IsDefined);
        Assert.Equal("Coordinates", result.MissingMember);
        Assert.False(result.IsNull);
        Assert.Equal("Address.Coordinates.Latitude", ctx.Key.PropertyPath);
        Assert.Equal("Latitude", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_NestedStructProperty_ReturnsValue_WhenAllPresent_AndCorrectPropertyKey()
    {
        var user = new User
        {
            Address = new Address
            {
                Coordinates = new Coordinates
                {
                    Latitude = 12.34,
                    Longitude = 56.78,
                    Info = new SomeInfo { Something = "test" }
                }
            }
        };

        // Access struct nested property (string? in SomeInfo)
        Expression<Func<User, string?>> expr = x => x.Address!.Coordinates!.Value.Info.Something;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(user);

        Assert.True(result.IsDefined);
        Assert.Null(result.MissingMember);
        Assert.False(result.IsNull);
        Assert.Equal("test", result.Value);
        Assert.Equal("Address.Coordinates.Info.Something", ctx.Key.PropertyPath);
        Assert.Equal("Something", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_StructValueAccess_ReturnsValue_WhenAllPresent_AndCorrectPropertyKey()
    {
        var user = new User
        {
            Address = new Address
            {
                Coordinates = new Coordinates
                {
                    Latitude = 12.34,
                    Longitude = 56.78
                }
            }
        };

        Expression<Func<User, double>> expr = x => x.Address!.Coordinates!.Value.Latitude;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(user);

        Assert.True(result.IsDefined);
        Assert.Null(result.MissingMember);
        Assert.False(result.IsNull);
        Assert.Equal(12.34, result.Value);
        Assert.Equal("Address.Coordinates.Latitude", ctx.Key.PropertyPath);
        Assert.Equal("Latitude", ctx.Key.PropertyName);
    }

    [Fact]
    public void CreateCtx_StructValueAccess_ReturnsUndefinedWithNoValue_WhenIntermediateNull_AndCorrectPropertyKey()
    {
        var user = new User
        {
            Address = null
        };

        Expression<Func<User, double>> expr = x => x.Address!.Coordinates!.Value.Latitude;
        var ctx = ExpressionFactory.CreateCtx(expr);

        var result = ctx.ExtractValue(user);

        Assert.False(result.IsDefined);
        Assert.Equal("Address", result.MissingMember);
        Assert.False(result.IsNull);
        Assert.Equal(default(double), result.Value);
        Assert.Equal("Address.Coordinates.Latitude", ctx.Key.PropertyPath);
        Assert.Equal("Latitude", ctx.Key.PropertyName);
    }
}
