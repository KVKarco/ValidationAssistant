using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using KVKarco.ValidationAssistant.Abstractions.ValidationContexts;
using KVKarco.ValidationAssistant.Exceptions;
using KVKarco.ValidationAssistant.Internal.Utilities;
using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using System.Diagnostics;
using System.Globalization;

namespace KVKarco.ValidationAssistant.UnitTests;

public class MessageTemplatePerformanceTests
{
    private const int Iterations = 100_000;
    private static readonly string[] TemplateNames = { "T1", "T2", "T3", "T4", "T5", "T6" };

    [Fact]
    public void Formatting_WithFiveTemplatesAndOneContext_IsFastAndEfficient()
    {
        // --- Arrange ---
        // Clear caches and register five different templates.
        InternalCache.ClearTemplates();

        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T1", CultureInfo.InvariantCulture, "Hello, {Name}!", true);
        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T2", CultureInfo.InvariantCulture, "The value is {Value}.", true);
        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T3", CultureInfo.InvariantCulture, "{FirstName} {LastName} is awesome!", true);
        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T4", CultureInfo.InvariantCulture, "{Number} is a great number.", true);
        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T5", CultureInfo.InvariantCulture, "The temperature is {Temp}°C.", true);
        ValidationAssistantConfig.GlobalDefaults.MessageTemplates.Register("T6", CultureInfo.InvariantCulture, "The date is {Date}.", true);
        DateTime dateTime = DateTime.UtcNow;

        // Use a single context for all test iterations and warmup 
        using var testContext = new FakeMessageContext(CultureInfo.InvariantCulture);

        //warmup
        testContext.GetTemplate(TemplateNames[0]).Replace("Name", "TestUser").GetMessage(true);
        testContext.GetTemplate(TemplateNames[1]).Replace("Value", 123).GetMessage(true);
        testContext.GetTemplate(TemplateNames[2]).Replace("FirstName", "John").Replace("LastName", "Doe").GetMessage(true);
        testContext.GetTemplate(TemplateNames[3]).Replace("Number", 999).GetMessage(true);
        testContext.GetTemplate(TemplateNames[4]).Replace("Temp", 25.5).GetMessage(true);
        testContext.GetTemplate(TemplateNames[5]).Replace("Date", dateTime).GetMessage(true);


        // --- Act & Measure ---
        Stopwatch stopwatch = Stopwatch.StartNew();
        long allocationsBefore = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < Iterations; i++)
        {
            string t1Message = testContext.GetTemplate(TemplateNames[0]).Replace("Name", "TestUser").GetMessage(true);
            string t2Message = testContext.GetTemplate(TemplateNames[1]).Replace("Value", 123).GetMessage(true);
            string t3Message = testContext.GetTemplate(TemplateNames[2]).Replace("FirstName", "John").Replace("LastName", "Doe").GetMessage(true);
            string t4Message = testContext.GetTemplate(TemplateNames[3]).Replace("Number", 999).GetMessage(true);
            string t5Message = testContext.GetTemplate(TemplateNames[4]).Replace("Temp", 25.5).GetMessage(true);
            string t6Message = testContext.GetTemplate(TemplateNames[5]).Replace("Date", dateTime).GetMessage(true);
        }

        long allocationsAfter = GC.GetAllocatedBytesForCurrentThread();
        stopwatch.Stop();

        long allocatedBytes = allocationsAfter - allocationsBefore;
        TimeSpan elapsed = stopwatch.Elapsed;

        // --- Assert ---
        Assert.True(elapsed.TotalSeconds < 1, $"Test took too long: {elapsed.TotalSeconds:F2} seconds.");
        Assert.True(allocatedBytes < 1024, $"Unexpected allocations: {allocatedBytes} bytes.");
    }
}

internal sealed class FakeMessageContext : IMessageCtx
{
    private MessageFormatter _messageFormatter;

    public FakeMessageContext(CultureInfo culture)
    {
        _messageFormatter = null!;
        Culture = culture;
    }

    public string PropertyName => "TestName";

    public CultureInfo Culture { get; }

    public IMessageResolver GetTemplate(string templateName)
    {
        _messageFormatter ??= InternalCache.RentFormatter();

        if (string.IsNullOrWhiteSpace(templateName))
            throw new ValidationCompositionException("Can`t get failure message template, templateName is missing.");

        CompiledTemplate? template = InternalCache.ResolveTemplate(templateName, Culture);

        return template is null
            ? throw new ValidationCompositionException($"Message template: '{templateName}', '{Culture.Name}' is not registered.")
            : _messageFormatter.ForNew(template);
    }

    public void Dispose()
    {
        InternalCache.ReturnFormatter(_messageFormatter);
    }
}