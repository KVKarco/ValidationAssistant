using KVKarco.ValidationAssistant.Common;
using KVKarco.ValidationAssistant.Common.Exceptions;
using KVKarco.ValidationAssistant.Common.Helpers;
using KVKarco.ValidationAssistant.TestModels;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressionFactoryTests
{
    private readonly Employee _employee;

    public ExpressionFactoryTests()
    {
        _employee = EmployeeFactory.CreateNewEmployeeWithOnlyHomeAddress();
    }

    [Fact]
    public void NaNEvaluator_Shuld_Retun_True()
    {
        string city = "Scranton";

        Expression<Func<Employee, string>> expression = e => e!.Info!.HomeAddress!.Value!.City!;

        Predicate<Employee> predicate = ExpressionFactory.CreateValueNaNEvaluator((Employee e) => e!.Info!.HomeAddress!.Value.City);

        bool hasValue = predicate(_employee);

        string result = expression.Compile()(_employee);

        Assert.True(hasValue);
        Assert.Equal(city, result);
    }

    [Fact]
    public void NaNEvaluator_Shuld_Retun_False()
    {
        Expression<Func<Employee, string>> expression = e => e!.Info!.WorkAddress!.Value!.City!;

        Predicate<Employee> predicate = ExpressionFactory.CreateValueNaNEvaluator(expression);

        bool hasValue = predicate(_employee);

        Assert.False(hasValue);
        Assert.Throws<InvalidOperationException>(() => expression.Compile()(_employee));
    }

    [Fact]
    public void PropertyKey_Extaction_Should_Throw_When_Contains_Methods_Calls()
    {
        Expression<Func<Restaurant, string>> expression = e => e!.Reservations!.First()!.CustomerName!;

        Assert.Throws<InvalidSelectorException>(() => ExpressionFactory.ExtractPropertyKey((MemberExpression)expression.Body));
    }

    [Fact]
    public void PropertyKey_Extaction_Should_Have_Path_WithOut_Values_Selected_From_NullableStructs()
    {
        string pathToProperty = "Address.Coordinates";
        string fullPath = "Address.Coordinates.Latitude";

        Expression<Func<Restaurant, int>> expression = e => e!.Address!.Value!.Coordinates!.Value.Latitude;

        PropertyKey key = ExpressionFactory.ExtractPropertyKey((MemberExpression)expression.Body);

        Assert.Equal(pathToProperty, key.PathToProperty);
        Assert.Equal(fullPath, key.FullPath);
    }

    [Fact]
    public void PropertyKey_Should_Throw_On_Entity_Selection()
    {
        Expression<Func<Address, Address>> expression = e => e;

        Assert.Throws<InvalidSelectorException>(() => expression.GetOrAddKey());
    }

    [Fact]
    public void PropertyKey_Should_Throw_On_Value_Selection_From_Nullable_Struct()
    {
        Restaurant restaurant = RestaurantFactory.CreateNewRestaurantWithDetails();

        Expression<Func<Restaurant, Address>> expression = e => e!.Address!.Value;

        Assert.Throws<InvalidSelectorException>(() => expression.GetOrAddKey());
    }
}