using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using System.Globalization;

namespace KVKarco.ValidationAssistant.UnitTests;

public class MessageTemplateRegistrarAndCacheTests
{
    private readonly MessageTemplateRegistrar _registrar;

    public MessageTemplateRegistrarAndCacheTests()
    {
        _registrar = new MessageTemplateRegistrar();
        // Clear the cache for each test to ensure isolation
        InternalCache.ClearTemplates();
    }

    [Fact]
    public void Register_ValidTemplate_AddsToCache()
    {
        // Arrange
        var templateName = "TestTemplate";
        var culture = CultureInfo.InvariantCulture;
        var message = "A simple message.";

        // Act
        _registrar.Register(templateName, culture, message);
        var resolvedTemplate = InternalCache.ResolveTemplate(templateName, culture);

        // Assert
        Assert.NotNull(resolvedTemplate);
        Assert.Equal(templateName, resolvedTemplate.Name);
    }

    [Fact]
    public void Register_WithOverwriteFalse_DoesNotOverwriteExisting()
    {
        // Arrange
        var templateName = "TestTemplate";
        var culture = CultureInfo.InvariantCulture;
        var message1 = "Original message.";
        var message2 = "New message.";

        // Act
        _registrar.Register(templateName, culture, message1);
        _registrar.Register(templateName, culture, message2, overwrite: false);
        var resolvedTemplate = InternalCache.ResolveTemplate(templateName, culture);

        // Assert
        Assert.NotNull(resolvedTemplate);
        Assert.Equal(message1, resolvedTemplate.Segments[0].Text);
    }

    [Theory]
    [InlineData("test1", null, "message")]
    [InlineData("test2", "en-US", null)]
    [InlineData(null, "en-US", "message")]
    [InlineData("", "en-US", "message")]
    public void Register_WithInvalidInput_ThrowsException(string name, string cultureName, string message)
    {
        // Arrange
        var culture = cultureName == null ? null : CultureInfo.GetCultureInfo(cultureName);

        // Act & Assert
        Assert.Throws<ValidationAssistantException>(() => _registrar.Register(name, culture!, message));
    }

    [Fact]
    public void RegisterFrom_WithValidSource_RegistersTemplates()
    {
        // Arrange
        var source = new TestMessageTemplateSource();

        // Act
        _registrar.RegisterFrom(source);

        // Assert
        var template = InternalCache.ResolveTemplate("TemplateFromSource", CultureInfo.InvariantCulture);
        Assert.NotNull(template);
        Assert.Equal("A message from the source.", template.Segments[0].Text);
    }

    [Fact]
    public void ResolveTemplate_WithExactCulture_ReturnsCorrectTemplate()
    {
        // Arrange
        _registrar.Register("Test", CultureInfo.GetCultureInfo("en-US"), "Hello, world!");

        // Act
        var template = InternalCache.ResolveTemplate("Test", CultureInfo.GetCultureInfo("en-US"));

        // Assert
        Assert.NotNull(template);
        Assert.Equal("Hello, world!", template.Segments[0].Text);
    }

    [Fact]
    public void ResolveTemplate_WithFallbackToParentCulture_ReturnsCorrectTemplate()
    {
        // Arrange
        _registrar.Register("Test", CultureInfo.GetCultureInfo("en"), "Hello, world!");

        // Act
        var template = InternalCache.ResolveTemplate("Test", CultureInfo.GetCultureInfo("en-US"));

        // Assert
        Assert.NotNull(template);
        Assert.Equal("Hello, world!", template.Segments[0].Text);
    }

    [Fact]
    public void ResolveTemplate_WithFallbackToInvariant_ReturnsCorrectTemplate()
    {
        // Arrange
        _registrar.Register("Test", CultureInfo.InvariantCulture, "Hello, world!");

        // Act
        var template = InternalCache.ResolveTemplate("Test", CultureInfo.GetCultureInfo("fr-CA"));

        // Assert
        Assert.NotNull(template);
        Assert.Equal("Hello, world!", template.Segments[0].Text);
    }

    private sealed class TestMessageTemplateSource : IMessageTemplateSource
    {
        public void Register(IMessageTemplateRegistrar registrar)
        {
            registrar.Register("TemplateFromSource", CultureInfo.InvariantCulture, "A message from the source.");
        }
    }
}