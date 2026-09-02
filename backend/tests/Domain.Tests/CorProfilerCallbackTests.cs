using System;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using Xunit;

namespace Domain.Tests;

public class CorProfilerCallbackTests
{
    [Fact]
    public void Initialize_WithValidObject_ReturnsSOk()
    {
        var callback = new CorProfilerCallbackSkeleton();
        var dummyInfo = new object();

        int hr = callback.Initialize(dummyInfo);

        Assert.Equal(CorProfilerHResults.S_OK, hr);
        Assert.True(callback.IsInitialized);
        Assert.False(callback.IsShutdown);
    }

    [Fact]
    public void Initialize_WithNull_ReturnsEInvalidArg()
    {
        var callback = new CorProfilerCallbackSkeleton();

        int hr = callback.Initialize(null!);

        Assert.Equal(CorProfilerHResults.E_INVALIDARG, hr);
        Assert.False(callback.IsInitialized);
    }

    [Fact]
    public void Shutdown_ReturnsSOkAndSetsShutdownFlag()
    {
        var callback = new CorProfilerCallbackSkeleton();
        callback.Initialize(new object());

        int hr = callback.Shutdown();

        Assert.Equal(CorProfilerHResults.S_OK, hr);
        Assert.True(callback.IsShutdown);
        Assert.False(callback.IsInitialized);
    }

    [Fact]
    public void Callbacks_DoNotThrowAndReturnSOk()
    {
        var callback = new CorProfilerCallbackSkeleton();

        Assert.Equal(CorProfilerHResults.S_OK, callback.AppDomainCreationStarted(IntPtr.Zero));
        Assert.Equal(CorProfilerHResults.S_OK, callback.AppDomainCreationFinished(IntPtr.Zero, CorProfilerHResults.S_OK));
        Assert.Equal(CorProfilerHResults.S_OK, callback.AssemblyLoadStarted(IntPtr.Zero));
        Assert.Equal(CorProfilerHResults.S_OK, callback.ModuleLoadStarted(IntPtr.Zero));
        Assert.Equal(CorProfilerHResults.S_OK, callback.ClassLoadStarted(IntPtr.Zero));
        Assert.Equal(CorProfilerHResults.S_OK, callback.JITCompilationStarted(IntPtr.Zero, true));
        Assert.Equal(CorProfilerHResults.S_OK, callback.JITCompilationFinished(IntPtr.Zero, CorProfilerHResults.S_OK, true));
    }
}
