using System;
using System.Reflection;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public static class ProbeMethodFinder
{
    public static MethodInfo? FindMethod(string methodPath)
    {
        if (string.IsNullOrWhiteSpace(methodPath))
        {
            return null;
        }

        int lastDot = methodPath.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == methodPath.Length - 1)
        {
            return null;
        }

        string className = methodPath[..lastDot];
        string methodName = methodPath[(lastDot + 1)..];

        Type? type = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                type = assembly.GetType(className);
                if (type != null)
                {
                    break;
                }
            }
            catch
            {
                // Handle reflection load failures gracefully
            }
        }

        if (type == null)
        {
            return null;
        }

        try
        {
            // Try to find the method using binding flags.
            // Support both public/non-public static/instance methods.
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (method != null)
            {
                return method;
            }
        }
        catch (AmbiguousMatchException)
        {
            // If overloaded, find the first method matching name.
            try
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    {
                        return m;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }
}
