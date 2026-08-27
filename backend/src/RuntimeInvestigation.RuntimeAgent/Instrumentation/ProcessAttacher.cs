using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public sealed class ProcessAttacher : IDisposable
{
    private Process? _attachedProcess;

    public Process? AttachedProcess => _attachedProcess;

    public void Attach(int pid)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("Process handle attachment is only supported on Windows.");
        }

        try
        {
            var process = Process.GetProcessById(pid);
            
            // Accessing Handle checks if we actually have access permission.
            _ = process.Handle;

            _attachedProcess = process;
            Console.WriteLine($"Successfully attached to PID {pid}");
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Process with PID {pid} is not running.", ex);
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
        {
            throw new UnauthorizedAccessException($"Access denied to PID {pid}.", ex);
        }
    }

    public string InspectRuntime()
    {
        if (_attachedProcess == null)
        {
            throw new InvalidOperationException("No process attached.");
        }

        try
        {
            // Force refresh process modules to get current loaded state.
            _attachedProcess.Refresh();
            
            var modules = _attachedProcess.Modules;
            foreach (ProcessModule module in modules)
            {
                var moduleName = module.ModuleName;
                if (string.Equals(moduleName, "coreclr.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return ".NET Core";
                }
                if (string.Equals(moduleName, "clr.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return ".NET Framework";
                }
            }
            return "Unknown";
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5)
        {
            throw new UnauthorizedAccessException("Access denied when reading process modules.", ex);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        _attachedProcess?.Dispose();
        _attachedProcess = null;
    }
}
