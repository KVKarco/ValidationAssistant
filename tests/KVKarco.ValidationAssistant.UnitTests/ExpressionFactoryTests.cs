using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Tests.Models;
using System.Linq.Expressions;

namespace KVKarco.ValidationAssistant.UnitTests;

public class ExpressionFactoryTests
{
    // Test methods will go here.

    [Fact]
    public void CreatePropertyCtx_NestedNonNullableStructProperty_ReturnsCorrectCtxAndUndefinedWithCorrectValue()
    {
        // Arrange
        TestUser user = TestUserDataFactory.CreateStaticUser(); // Use static user for predictable values
        Expression<Func<TestUser, bool>> selector = u => u.PrimaryContact!.UserPreferences!.AppSettings!.Value.IsDarkMode;

        // Act
        var propertyCtx = ExpressionFactory.CreatePropertyCtx(selector, false, false);
        var resolvedValue = propertyCtx.ExtractValue(user);

        // Assert
        Assert.NotNull(propertyCtx);
        Assert.Equal(nameof(Settings.IsDarkMode), propertyCtx.Key.PropertyName); // Corrected to PropertyName
        Assert.Equal("PrimaryContact.UserPreferences.AppSettings.IsDarkMode", propertyCtx.Key.PropertyPath);
        Assert.False(propertyCtx.Key.IsForCollection);

        Assert.True(resolvedValue.HasValue);
        Assert.False(resolvedValue.IsNull);
        Assert.Equal(user.PrimaryContact!.UserPreferences!.AppSettings!.Value.IsDarkMode, resolvedValue.Value);
    }

    [Fact]
    public void CreatePropertyCtx_NestedNullableReferenceProperty_ReturnsCorrectCtxAndUndefinedWithCorrectValue()
    {
        // Arrange
        TestUser user = TestUserDataFactory.CreateStaticUser(); // Use static user for predictable values
        Expression<Func<TestUser, string?>> selector = u => u.Address!.State!.Name;

        // Act
        var propertyCtx = ExpressionFactory.CreatePropertyCtx(selector, false, false);
        var resolvedValue = propertyCtx.ExtractValue(user);

        // Assert
        Assert.NotNull(propertyCtx);
        Assert.Equal(nameof(StateInfo.Name), propertyCtx.Key.PropertyName); // Corrected to PropertyName
        Assert.Equal("Address.State.Name", propertyCtx.Key.PropertyPath);
        Assert.False(propertyCtx.Key.IsForCollection);

        Assert.True(resolvedValue.HasValue);
        Assert.False(resolvedValue.IsNull); // "California" is not null
        Assert.Equal(user.Address!.State!.Name, resolvedValue.Value);
    }

    [Fact]
    public void CreatePropertyCtx_NestedNullableReferenceProperty_ReturnsCorrectCtxAndUndefinedWithHasValue_False()
    {
        // Arrange
        TestUser userWithNullAddress = TestUserDataFactory.CreateStaticUser();
        userWithNullAddress.Address = null; // Set intermediate property to null
        Expression<Func<TestUser, string?>> selector = u => u.Address!.State!.Name;

        // Act
        var propertyCtx = ExpressionFactory.CreatePropertyCtx(selector, false, false);
        var resolvedValue = propertyCtx.ExtractValue(userWithNullAddress);

        // Assert
        Assert.NotNull(propertyCtx);
        Assert.Equal(nameof(StateInfo.Name), propertyCtx.Key.PropertyName); // Corrected to PropertyName
        Assert.Equal("Address.State.Name", propertyCtx.Key.PropertyPath);
        Assert.False(propertyCtx.Key.IsForCollection);

        Assert.False(resolvedValue.HasValue); // Should be undefined due to null Address
        Assert.True(resolvedValue.IsNull); // Corrected: IsNull should be true when HasValue is false
        Assert.Equal(default(string), resolvedValue.Value); // Value should be default for TProperty
    }

    [Fact]
    public void CreatePropertyCtx_NestedNullableReferenceProperty_ReturnsCorrectCtxAndUndefinedWithCorrectNullValue()
    {
        // Arrange
        var userWithNullStateName = TestUserDataFactory.CreateStaticUser();
        userWithNullStateName.Address!.State!.Name = null; // Set final property to null
        Expression<Func<TestUser, string?>> selector = u => u.Address!.State!.Name;

        // Act
        var propertyCtx = ExpressionFactory.CreatePropertyCtx(selector, false, false);
        var resolvedValue = propertyCtx.ExtractValue(userWithNullStateName);

        // Assert
        Assert.NotNull(propertyCtx);
        Assert.Equal(nameof(StateInfo.Name), propertyCtx.Key.PropertyName); // Corrected to PropertyName
        Assert.Equal("Address.State.Name", propertyCtx.Key.PropertyPath);
        Assert.False(propertyCtx.Key.IsForCollection);

        Assert.True(resolvedValue.HasValue); // HasValue should be true, as the path is resolved
        Assert.True(resolvedValue.IsNull);   // IsNull should be true, as the final value is null
        Assert.Null(resolvedValue.Value);    // Value should be null
    }

    [Fact]
    public void CreatePropertyCtx_SelectorWithMethodCall_ThrowsRuleCreationException()
    {
        // Arrange
        // Attempting to use a method call (e.g., .Substring) in the selector chain
        Expression<Func<TestUser, int>> selector = u => u.Address!.State!.Name!.Substring(2).Length;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.CreatePropertyCtx(selector, false, false);
        });

        Assert.Contains("Only selecting properties or fields is allowed in selectors.", exception.Message);
    }

    [Fact]
    public void CreatePropertyCtx_SelectorWithExplicitCast_ThrowsRuleCreationException()
    {
        // Arrange
        // Attempting to use an explicit cast in the selector chain
        Expression<Func<TestUser, bool>> selector = u => ((Settings)u.PrimaryContact!.UserPreferences!.AppSettings!).IsDarkMode;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.CreatePropertyCtx(selector, false, false);
        });

        // The exact message might vary depending on where the validation fails in the traversal.
        // It could be about the body not being a MemberExpression, or an intermediate node.
        Assert.Contains("Only selecting properties or fields is allowed in selectors.", exception.Message);
    }

    // New tests for GetLastMemberAndMembersCount method

    [Fact]
    public void GetLastMemberAndMembersCount_SingleProperty_ReturnsCorrectInfo()
    {
        // Arrange
        Expression<Func<TestUser, string?>> selector = user => user.Name;

        // Act
        var (memberInfo, count) = ExpressionFactory.GetFirstMemberInChainAndCount(selector);

        // Assert
        Assert.NotNull(memberInfo);
        Assert.Equal(nameof(TestUser.Name), memberInfo.Name);
        Assert.Equal(1, count);
    }

    [Fact]
    public void GetLastMemberAndMembersCount_MultiLevelProperty_ReturnsCorrectInfo()
    {
        // Arrange
        Expression<Func<TestUser, string?>> selector = user => user.Address!.State!.Name;

        // Act
        var (memberInfo, count) = ExpressionFactory.GetFirstMemberInChainAndCount(selector);

        // Assert
        Assert.NotNull(memberInfo);
        Assert.Equal(nameof(TestUser.Address), memberInfo.Name);
        Assert.Equal(3, count); // User.Address.State.Name (Address, State, Name)
    }

    [Fact]
    public void GetLastMemberAndMembersCount_NullableValueTypeProperty_ReturnsCorrectInfo()
    {
        // Arrange
        Expression<Func<TestUser, DateTime?>> selector = user => user.Birthday;

        // Act
        var (memberInfo, count) = ExpressionFactory.GetFirstMemberInChainAndCount(selector);

        // Assert
        Assert.NotNull(memberInfo);
        Assert.Equal(nameof(TestUser.Birthday), memberInfo.Name);
        Assert.Equal(1, count);
    }

    [Fact]
    public void GetLastMemberAndMembersCount_NullableStructProperty_ReturnsCorrectInfo()
    {
        // Arrange
        // Selector for the nullable struct itself, not its members
        Expression<Func<TestUser, Settings?>> selector = user => user.PrimaryContact!.UserPreferences!.AppSettings;

        // Act
        var (memberInfo, count) = ExpressionFactory.GetFirstMemberInChainAndCount(selector);

        // Assert
        Assert.NotNull(memberInfo);
        Assert.Equal(nameof(TestUser.PrimaryContact), memberInfo.Name);
        Assert.Equal(3, count); // PrimaryContact.UserPreferences.AppSettings
    }

    [Fact]
    public void GetLastMemberAndMembersCount_EnumProperty_ReturnsCorrectInfo()
    {
        // Arrange
        Expression<Func<TestUser, UserStatus>> selector = user => user.Status;

        // Act
        var (memberInfo, count) = ExpressionFactory.GetFirstMemberInChainAndCount(selector);

        // Assert
        Assert.NotNull(memberInfo);
        Assert.Equal(nameof(TestUser.Status), memberInfo.Name);
        Assert.Equal(1, count);
    }

    [Fact]
    public void GetLastMemberAndMembersCount_SelectorWithMethodCall_ThrowsRuleCreationException()
    {
        // Arrange
        Expression<Func<TestUser, int>> selector = u => u.Name!.Substring(2).Length; // Method call

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.GetFirstMemberInChainAndCount(selector);
        });

        Assert.Contains("Only selecting properties or fields is allowed in selectors.", exception.Message);
    }

    [Fact]
    public void GetFirstMemberAndMembersCount_SelectorWithExplicitCast_ThrowsRuleCreationException()
    {
        // Arrange
        Expression<Func<TestUser, bool>> selector = u => ((Settings)u.PrimaryContact!.UserPreferences!.AppSettings!).IsDarkMode;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.GetFirstMemberInChainAndCount(selector);
        });

        Assert.Contains("Only selecting properties or fields is allowed in selectors.", exception.Message);
    }

    [Fact]
    public void CreatePropertyCtx_SelectorIsParameter_ThrowsRuleCreationException()
    {
        // Arrange
        // Selector is just the parameter itself (u => u)
        Expression<Func<TestUser, TestUser>> selector = u => u;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.CreatePropertyCtx(selector, false, false);
        });

        Assert.Contains("Cannot select the main instance for validation directly. A property or field must be selected.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreatePropertyCtx_SelectorIsConstant_ThrowsRuleCreationException()
    {
        // Arrange
        // Selector is a constant value (u => 42)
        Expression<Func<TestUser, int>> selector = u => 42;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.CreatePropertyCtx(selector, false, false);
        });

        Assert.Contains("Only selecting properties or fields is allowed.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreatePropertyCtx_SelectorIsOperation_ThrowsRuleCreationException()
    {
        // Arrange
        // Selector is an operation (u => u.Age + 1), assuming Age is a property
        Expression<Func<TestUser, int>> selector = u => u.Age + 1;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.CreatePropertyCtx(selector, false, false);
        });

        Assert.Contains("Only selecting properties or fields is allowed.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFirstMemberInChainAndCount_SelectorIsParameter_ThrowsRuleCreationException()
    {
        // Arrange
        Expression<Func<TestUser, TestUser>> selector = u => u;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.GetFirstMemberInChainAndCount(selector);
        });

        Assert.Contains("Cannot select the main instance for validation directly. A property or field must be selected.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFirstMemberInChainAndCount_SelectorIsConstant_ThrowsRuleCreationException()
    {
        // Arrange
        Expression<Func<TestUser, int>> selector = u => 42;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.GetFirstMemberInChainAndCount(selector);
        });

        Assert.Contains("Only selecting properties or fields is allowed.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFirstMemberInChainAndCount_SelectorIsOperation_ThrowsRuleCreationException()
    {
        // Arrange
        Expression<Func<TestUser, int>> selector = u => u.Age + 1;

        // Act & Assert
        var exception = Assert.Throws<RuleCreationException>(() =>
        {
            ExpressionFactory.GetFirstMemberInChainAndCount(selector);
        });

        Assert.Contains("Only selecting properties or fields is allowed.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
