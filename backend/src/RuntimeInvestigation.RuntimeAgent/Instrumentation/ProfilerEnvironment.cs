using System;
using System.Collections.Generic;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public enum TargetClrRuntime
{
    CoreClr,
    NetFramework
}

public sealed class ProfilerEnvironmentConfig
{
    public string ProfilerGuid { get; init; } = "{23C6D378-9F3C-4364-9842-88E07936E7B6}";
    public string ProfilerPath { get; init; } = string.Empty;
    public TargetClrRuntime Runtime { get; init; } = TargetClrRuntime.CoreClr;
    public bool EnableProfiling { get; init; } = true;
}

public static class ProfilerEnvironment
{
    public const string CoreClrEnableVar = "CORECLR_ENABLE_PROFILING";
    public const string CoreClrProfilerVar = "CORECLR_PROFILER";
    public const string CoreClrProfilerPathVar = "CORECLR_PROFILER_PATH";

    public const string NetFrameworkEnableVar = "COR_ENABLE_PROFILING";
    public const string NetFrameworkProfilerVar = "COR_PROFILER";
    public const string NetFrameworkProfilerPathVar = "COR_PROFILER_PATH";

    public static IReadOnlyDictionary<string, string> GenerateEnvironmentVariables(ProfilerEnvironmentConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ProfilerPath))
        {
            throw new ArgumentException("ProfilerPath is required and cannot be empty.", nameof(config));
        }

        var env = new Dictionary<string, string>();

        if (config.Runtime == TargetClrRuntime.CoreClr)
        {
            env[CoreClrEnableVar] = config.EnableProfiling ? "1" : "0";
            env[CoreClrProfilerVar] = config.ProfilerGuid;
            env[CoreClrProfilerPathVar] = config.ProfilerPath;
        }
        else if (config.Runtime == TargetClrRuntime.NetFramework)
        {
            env[NetFrameworkEnableVar] = config.EnableProfiling ? "1" : "0";
            env[NetFrameworkProfilerVar] = config.ProfilerGuid;
            env[NetFrameworkProfilerPathVar] = config.ProfilerPath;
        }

        return env;
    }

    public static TargetClrRuntime DetectRuntimeFromModuleName(string moduleName)
    {
        if (string.Equals(moduleName, "coreclr.dll", StringComparison.OrdinalIgnoreCase))
        {
            return TargetClrRuntime.CoreClr;
        }

        if (string.Equals(moduleName, "clr.dll", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(moduleName, "mscorwks.dll", StringComparison.OrdinalIgnoreCase))
        {
            return TargetClrRuntime.NetFramework;
        }

        return TargetClrRuntime.CoreClr;
    }
}
