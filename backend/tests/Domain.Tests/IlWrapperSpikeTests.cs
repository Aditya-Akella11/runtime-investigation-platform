using System.Reflection;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;

namespace Domain.Tests;

/// <summary>
/// Ticket 8 - IL wrapper tests.
/// Tests execution, correct return values, signature preservation, and entry logging.
/// </summary>
public class IlWrapperSpikeTests
{
    // Target static method for testing
    public static string ToUpperHelper(string input) => input.ToUpperInvariant();
    public static int AddHelper(int a, int b) => a + b;
    internal static void VoidHelper(string _) { /* intentionally void */ }

    [Fact]
    public void CreateWrapper_StaticStringMethod_ExecutesAndReturnsCorrectly()
    {
        var entryLog = new List<(string method, object?[] args)>();
        var exitLog = new List<(string method, object? result)>();

        IlWrapperSpike.OnMethodEntry = (m, a) => entryLog.Add((m, a));
        IlWrapperSpike.OnMethodExit = (m, r) => exitLog.Add((m, r));

        var targetMethod = typeof(IlWrapperSpikeTests).GetMethod(
            nameof(ToUpperHelper),
            BindingFlags.Public | BindingFlags.Static)!;

        var wrapper = IlWrapperSpike.CreateWrapper(targetMethod);
        var result = wrapper.DynamicInvoke("hello");

        Assert.Equal("HELLO", result);
        Assert.Single(entryLog);
        Assert.Contains("ToUpperHelper", entryLog[0].method);
        Assert.Single(exitLog);
        Assert.Equal("HELLO", exitLog[0].result);

        IlWrapperSpike.OnMethodEntry = null;
        IlWrapperSpike.OnMethodExit = null;
    }

    [Fact]
    public void CreateWrapper_StaticIntMethod_ExecutesAndReturnsCorrectly()
    {
        var entryLog = new List<(string method, object?[] args)>();

        IlWrapperSpike.OnMethodEntry = (m, a) => entryLog.Add((m, a));
        IlWrapperSpike.OnMethodExit = null;

        var targetMethod = typeof(IlWrapperSpikeTests).GetMethod(
            nameof(AddHelper),
            BindingFlags.Public | BindingFlags.Static)!;

        var wrapper = IlWrapperSpike.CreateWrapper(targetMethod);
        var result = wrapper.DynamicInvoke(5, 7);

        Assert.Equal(12, result);
        Assert.Single(entryLog);

        IlWrapperSpike.OnMethodEntry = null;
    }

    [Fact]
    public void CreateWrapper_VoidMethod_ExecutesWithoutException()
    {
        var entryLog = new List<string>();

        IlWrapperSpike.OnMethodEntry = (m, _) => entryLog.Add(m);
        IlWrapperSpike.OnMethodExit = null;

        var targetMethod = typeof(IlWrapperSpikeTests).GetMethod(
            nameof(VoidHelper),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;

        var wrapper = IlWrapperSpike.CreateWrapper(targetMethod);
        var ex = Record.Exception(() => wrapper.DynamicInvoke("test-input"));

        Assert.Null(ex);
        Assert.Single(entryLog);
        Assert.Contains("VoidHelper", entryLog[0]);

        IlWrapperSpike.OnMethodEntry = null;
    }

    [Fact]
    public void CreateWrapper_StringToUpper_PreservesSignature()
    {
        var method = typeof(string).GetMethod("ToUpperInvariant", Type.EmptyTypes)!;
        var wrapper = IlWrapperSpike.CreateWrapper(method);

        // Signature preserved: the delegate should take a string (instance) and return string
        Assert.NotNull(wrapper);
        var result = wrapper.DynamicInvoke("lowercase");
        Assert.Equal("LOWERCASE", result);
    }

    [Fact]
    public void LogEntry_IsCalledOnInvocation()
    {
        int callCount = 0;
        IlWrapperSpike.OnMethodEntry = (_, __) => callCount++;
        IlWrapperSpike.OnMethodExit = null;

        var method = typeof(IlWrapperSpikeTests).GetMethod(
            nameof(AddHelper),
            BindingFlags.Public | BindingFlags.Static)!;

        var wrapper = IlWrapperSpike.CreateWrapper(method);
        wrapper.DynamicInvoke(1, 2);

        Assert.Equal(1, callCount);
        IlWrapperSpike.OnMethodEntry = null;
    }

    [Fact]
    public void LogExit_IsCalledOnSuccessfulReturn()
    {
        int callCount = 0;
        IlWrapperSpike.OnMethodEntry = null;
        IlWrapperSpike.OnMethodExit = (_, __) => callCount++;

        var method = typeof(IlWrapperSpikeTests).GetMethod(
            nameof(ToUpperHelper),
            BindingFlags.Public | BindingFlags.Static)!;

        var wrapper = IlWrapperSpike.CreateWrapper(method);
        wrapper.DynamicInvoke("test");

        Assert.Equal(1, callCount);
        IlWrapperSpike.OnMethodExit = null;
    }
}
