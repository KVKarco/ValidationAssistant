using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using System.Globalization;

namespace KVKarco.ValidationAssistant.UnitTests;

public class MessageFormatterTests
{
    private readonly CompiledTemplate _template;

    public MessageFormatterTests()
    {
        _template = CompiledTemplate.Parse("testTemplate", CultureInfo.InvariantCulture, "Hello, {Name}! The value is {Value}.");
    }

    [Fact]
    public void ForNew_ResetsFormatterForNewTemplate()
    {
        // Arrange
        var formatter = new MessageFormatter();
        var template2 = CompiledTemplate.Parse("test2", CultureInfo.InvariantCulture, "Goodbye, {Name}!");

        // Act
        var result1 = formatter.ForNew(_template).Replace("Name", "Alice").Replace("Value", "TestValue").GetMessage();
        var result2 = formatter.ForNew(template2).Replace("Name", "Bob").GetMessage();

        // Assert
        Assert.Equal("Hello, Alice! The value is TestValue.", result1);
        Assert.Equal("Goodbye, Bob!", result2);
    }

    [Fact]
    public void Replace_WithValidPlaceholder_FormatsCorrectly()
    {
        // Arrange
        var formatter = new MessageFormatter();

        // Act
        string message = formatter.ForNew(_template)
            .Replace("Name", "Alice")
            .Replace("Value", 123)
            .GetMessage();

        // Assert
        Assert.Equal("Hello, Alice! The value is 123.", message);
    }

    [Fact]
    public void GetMessage_WithoutAllPlaceholdersReplaced_ThrowsCorrectExceptionWithMessage()
    {
        // Arrange
        var formatter = new MessageFormatter();

        // Act & Assert
        // Assert.Throws captures the thrown exception and holds a reference to it.
        var ex = Assert.Throws<ValidationAssistantException>(() =>
        {
            formatter.ForNew(_template)
                     .Replace("Name", "Alice")
                     .GetMessage();
        });

        // Now, we can assert on the exception's properties.
        // We use Assert.Contains to check for a key part of the message, making the test less brittle.
        Assert.Equal(
            $"Message template: '{_template.Name}' contains '{_template.PlaceholdersAvailable}' please provide replacements for them all before getting the message.",
            ex.Message);
    }

    [Fact]
    public void GetMessage_WithoutAnyPlaceholdersReplaced_ThrowsCorrectExceptionWithMessage()
    {
        // Arrange
        var formatter = new MessageFormatter();

        // Act & Assert
        // Assert.Throws captures the thrown exception and holds a reference to it.
        var ex = Assert.Throws<ValidationAssistantException>(() =>
        {
            formatter.ForNew(_template)
                     .GetMessage();
        });

        // Now, we can assert on the exception's properties.
        // We use Assert.Contains to check for a key part of the message, making the test less brittle.
        Assert.Equal(
            $"Message template: '{_template.Name}' contains '{_template.PlaceholdersAvailable}' please provide replacements for them all before getting the message.",
            ex.Message);
    }

    [Fact]
    public void GetMessage_WithExcessiveLength_CorrectlyResizesBuffer()
    {
        // Arrange
        var template = CompiledTemplate.Parse("large", CultureInfo.InvariantCulture, "1234567890{Name}1234567890");
        var formatter = new MessageFormatter();
        var longName = new string('A', 500); // Exceeds initial buffer size

        // Act
        string message = formatter.ForNew(template)
            .Replace("Name", longName)
            .GetMessage();

        // Assert
        Assert.Equal($"1234567890{longName}1234567890", message);
    }

    [Fact]
    public void GetMessage_WithNonSpanFormattableObject_UsesToString()
    {
        // Arrange
        var formatter = new MessageFormatter();
        var testObject = new object();

        // Act
        string message = formatter.ForNew(_template)
            .Replace("Name", "TestName")
            .Replace("Value", testObject)
            .GetMessage();

        // Assert
        Assert.Equal($"Hello, TestName! The value is {testObject}.", message);
    }

    [Fact]
    public void Dispose_ReturnsBufferToPool()
    {
        // This is a difficult test to write directly without a custom mock pool.
        // We can simulate it by observing behavior.
        // Arrange
        var formatter = new MessageFormatter();

        // Act
        formatter.Dispose();

        // Assert
        // After dispose, the internal buffer should be null or returned to the pool.
        // Accessing the private field here for verification.
        Assert.True(formatter.IsBufferReturned);
    }
}