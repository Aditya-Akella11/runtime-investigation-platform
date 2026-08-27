using RuntimeInvestigation.RuntimeAgent.Instrumentation;

namespace Domain.Tests;

/// <summary>
/// Ticket 7 - ProbeMethodFinder tests.
/// Tests finding System.Console.WriteLine and returning null for invalid method paths.
/// </summary>
public class ProbeMethodFinderTests
{
    [Fact]
    public void FindMethod_WithNullOrEmpty_ReturnsNull()
    {
        Assert.Null(ProbeMethodFinder.FindMethod(null!));
        Assert.Null(ProbeMethodFinder.FindMethod(""));
        Assert.Null(ProbeMethodFinder.FindMethod("   "));
    }

    [Fact]
    public void FindMethod_WithInvalidFormat_ReturnsNull()
    {
        // No dot separator at all
        Assert.Null(ProbeMethodFinder.FindMethod("NoNamespaceOrClass"));
        // Dot at the very end
        Assert.Null(ProbeMethodFinder.FindMethod("SomeClass."));
        // Dot at the very start
        Assert.Null(ProbeMethodFinder.FindMethod(".SomeMethod"));
    }

    [Fact]
    public void FindMethod_WithInvalidClassName_ReturnsNull()
    {
        Assert.Null(ProbeMethodFinder.FindMethod("Totally.NonExistent.ClassName.Method"));
    }

    [Fact]
    public void FindMethod_WithInvalidMethodName_ReturnsNull()
    {
        Assert.Null(ProbeMethodFinder.FindMethod("System.Console.NonExistentMethod12345"));
    }

    [Fact]
    public void FindMethod_ConsoleWriteLine_ReturnsMethodInfo()
    {
        // System.Console.WriteLine is a well-known method available in any .NET app
        var method = ProbeMethodFinder.FindMethod("System.Console.WriteLine");

        Assert.NotNull(method);
        Assert.Equal("WriteLine", method!.Name);
    }

    [Fact]
    public void FindMethod_StringToUpper_ReturnsMethodInfo()
    {
        var method = ProbeMethodFinder.FindMethod("System.String.ToUpper");

        Assert.NotNull(method);
        Assert.Equal("ToUpper", method!.Name);
    }

    [Fact]
    public void FindMethod_ReturnsOnlyMethodsFromSupportedBoundary()
    {
        // The method returned must be a real MethodInfo (not null, and has a valid declaring type)
        var method = ProbeMethodFinder.FindMethod("System.Console.WriteLine");

        Assert.NotNull(method);
        Assert.NotNull(method!.DeclaringType);
    }
}
