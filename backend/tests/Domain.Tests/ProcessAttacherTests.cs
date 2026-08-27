using System.Runtime.InteropServices;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;

namespace Domain.Tests;

/// <summary>
/// Ticket 6 - ProcessAttacher tests.
/// Tests attaching to the current process and graceful failure for an invalid PID.
/// Platform-specific tests are skipped when Windows process APIs are unavailable.
/// </summary>
public class ProcessAttacherTests
{
    [Fact]
    public void Attach_ToCurrentProcess_Succeeds_OnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows platforms
        }

        var attacher = new ProcessAttacher();
        int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;

        attacher.Attach(currentPid);

        Assert.NotNull(attacher.AttachedProcess);
        Assert.Equal(currentPid, attacher.AttachedProcess!.Id);

        attacher.Dispose();
    }

    [Fact]
    public void Attach_ToInvalidPid_ThrowsArgumentException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows platforms
        }

        var attacher = new ProcessAttacher();
        // Use an extremely unlikely PID that should not exist
        int invalidPid = 9999999;

        var ex = Assert.ThrowsAny<Exception>(() => attacher.Attach(invalidPid));
        Assert.True(ex is ArgumentException || ex is InvalidOperationException);

        attacher.Dispose();
    }

    [Fact]
    public void Attach_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // This test only applies on non-Windows
        }

        var attacher = new ProcessAttacher();
        Assert.Throws<PlatformNotSupportedException>(() => attacher.Attach(1));
        attacher.Dispose();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_WithoutException()
    {
        var attacher = new ProcessAttacher();
        // Should not throw when disposed without attachment
        attacher.Dispose();
        attacher.Dispose();
    }

    [Fact]
    public void InspectRuntime_WithoutAttachment_ThrowsInvalidOperationException()
    {
        var attacher = new ProcessAttacher();
        Assert.Throws<InvalidOperationException>(() => attacher.InspectRuntime());
    }

    [Fact]
    public void InspectRuntime_AfterAttachToCurrentProcess_ReturnsNonEmptyString()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }

        using var attacher = new ProcessAttacher();
        int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;
        attacher.Attach(currentPid);

        var runtime = attacher.InspectRuntime();

        Assert.NotNull(runtime);
        Assert.NotEmpty(runtime);
        // The current process is .NET 8, so it should detect coreclr.dll
        Assert.Contains(".NET", runtime, StringComparison.OrdinalIgnoreCase);
    }
}
