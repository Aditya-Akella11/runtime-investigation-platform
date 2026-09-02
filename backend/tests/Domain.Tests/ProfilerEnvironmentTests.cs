using System;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using Xunit;

namespace Domain.Tests;

public class ProfilerEnvironmentTests
{
    [Fact]
    public void GenerateEnvironmentVariables_CoreClr_SetsCoreClrVars()
    {
        var config = new ProfilerEnvironmentConfig
        {
            ProfilerGuid = "{12345678-1234-1234-1234-1234567890AB}",
            ProfilerPath = @"C:\agent\profiler.dll",
            Runtime = TargetClrRuntime.CoreClr,
            EnableProfiling = true
        };

        var env = ProfilerEnvironment.GenerateEnvironmentVariables(config);

        Assert.Equal("1", env[ProfilerEnvironment.CoreClrEnableVar]);
        Assert.Equal("{12345678-1234-1234-1234-1234567890AB}", env[ProfilerEnvironment.CoreClrProfilerVar]);
        Assert.Equal(@"C:\agent\profiler.dll", env[ProfilerEnvironment.CoreClrProfilerPathVar]);
        Assert.False(env.ContainsKey(ProfilerEnvironment.NetFrameworkEnableVar));
    }

    [Fact]
    public void GenerateEnvironmentVariables_NetFramework_SetsCorVars()
    {
        var config = new ProfilerEnvironmentConfig
        {
            ProfilerGuid = "{12345678-1234-1234-1234-1234567890AB}",
            ProfilerPath = @"C:\agent\cor_profiler.dll",
            Runtime = TargetClrRuntime.NetFramework,
            EnableProfiling = true
        };

        var env = ProfilerEnvironment.GenerateEnvironmentVariables(config);

        Assert.Equal("1", env[ProfilerEnvironment.NetFrameworkEnableVar]);
        Assert.Equal("{12345678-1234-1234-1234-1234567890AB}", env[ProfilerEnvironment.NetFrameworkProfilerVar]);
        Assert.Equal(@"C:\agent\cor_profiler.dll", env[ProfilerEnvironment.NetFrameworkProfilerPathVar]);
        Assert.False(env.ContainsKey(ProfilerEnvironment.CoreClrEnableVar));
    }

    [Fact]
    public void GenerateEnvironmentVariables_EmptyPath_ThrowsArgumentException()
    {
        var config = new ProfilerEnvironmentConfig
        {
            ProfilerPath = ""
        };

        Assert.Throws<ArgumentException>(() => ProfilerEnvironment.GenerateEnvironmentVariables(config));
    }

    [Theory]
    [InlineData("coreclr.dll", TargetClrRuntime.CoreClr)]
    [InlineData("clr.dll", TargetClrRuntime.NetFramework)]
    [InlineData("mscorwks.dll", TargetClrRuntime.NetFramework)]
    public void DetectRuntimeFromModuleName_ReturnsCorrectRuntime(string moduleName, TargetClrRuntime expected)
    {
        var runtime = ProfilerEnvironment.DetectRuntimeFromModuleName(moduleName);
        Assert.Equal(expected, runtime);
    }
}
