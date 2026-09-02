using System;
using System.Runtime.InteropServices;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public static class CorProfilerHResults
{
    public const int S_OK = 0;
    public const int E_FAIL = unchecked((int)0x80004005);
    public const int E_INVALIDARG = unchecked((int)0x80070057);
    public const int E_NOTIMPL = unchecked((int)0x80004001);
    public const int CORPROF_E_PROFILER_CANCEL_ACTIVATION = unchecked((int)0x80131362);
}

[Guid("176FBED1-A55C-4796-98B2-C97AEAA0109C")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ICorProfilerCallback
{
    [PreserveSig]
    int Initialize([MarshalAs(UnmanagedType.IUnknown)] object pICorProfilerInfoUnk);

    [PreserveSig]
    int Shutdown();

    [PreserveSig]
    int AppDomainCreationStarted(IntPtr appDomainId);

    [PreserveSig]
    int AppDomainCreationFinished(IntPtr appDomainId, int hrStatus);

    [PreserveSig]
    int AppDomainShutdownStarted(IntPtr appDomainId);

    [PreserveSig]
    int AppDomainShutdownFinished(IntPtr appDomainId, int hrStatus);

    [PreserveSig]
    int AssemblyLoadStarted(IntPtr assemblyId);

    [PreserveSig]
    int AssemblyLoadFinished(IntPtr assemblyId, int hrStatus);

    [PreserveSig]
    int ModuleLoadStarted(IntPtr moduleId);

    [PreserveSig]
    int ModuleLoadFinished(IntPtr moduleId, int hrStatus);

    [PreserveSig]
    int ModuleAttachedToAssembly(IntPtr moduleId, IntPtr AssemblyId);

    [PreserveSig]
    int ClassLoadStarted(IntPtr classId);

    [PreserveSig]
    int ClassLoadFinished(IntPtr classId, int hrStatus);

    [PreserveSig]
    int JITCompilationStarted(IntPtr functionId, bool fIsSafeToBlock);

    [PreserveSig]
    int JITCompilationFinished(IntPtr functionId, int hrStatus, bool fIsSafeToBlock);

    [PreserveSig]
    int JITCachedFunctionSearchStarted(IntPtr functionId, out bool pbUseCachedFunction);

    [PreserveSig]
    int JITCachedFunctionSearchFinished(IntPtr functionId, int hrStatus);

    [PreserveSig]
    int JITFunctionPitched(IntPtr functionId);

    [PreserveSig]
    int JITInlining(IntPtr callerId, IntPtr calleeId, out bool pfShouldInline);
}

[Guid("23C6D378-9F3C-4364-9842-88E07936E7B6")]
[ComVisible(true)]
public class CorProfilerCallbackSkeleton : ICorProfilerCallback
{
    public bool IsInitialized { get; private set; }
    public bool IsShutdown { get; private set; }

    public virtual int Initialize(object pICorProfilerInfoUnk)
    {
        if (pICorProfilerInfoUnk == null)
        {
            return CorProfilerHResults.E_INVALIDARG;
        }

        IsInitialized = true;
        IsShutdown = false;
        return CorProfilerHResults.S_OK;
    }

    public virtual int Shutdown()
    {
        IsShutdown = true;
        IsInitialized = false;
        return CorProfilerHResults.S_OK;
    }

    public virtual int AppDomainCreationStarted(IntPtr appDomainId) => CorProfilerHResults.S_OK;
    public virtual int AppDomainCreationFinished(IntPtr appDomainId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int AppDomainShutdownStarted(IntPtr appDomainId) => CorProfilerHResults.S_OK;
    public virtual int AppDomainShutdownFinished(IntPtr appDomainId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int AssemblyLoadStarted(IntPtr assemblyId) => CorProfilerHResults.S_OK;
    public virtual int AssemblyLoadFinished(IntPtr assemblyId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int ModuleLoadStarted(IntPtr moduleId) => CorProfilerHResults.S_OK;
    public virtual int ModuleLoadFinished(IntPtr moduleId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int ModuleAttachedToAssembly(IntPtr moduleId, IntPtr AssemblyId) => CorProfilerHResults.S_OK;
    public virtual int ClassLoadStarted(IntPtr classId) => CorProfilerHResults.S_OK;
    public virtual int ClassLoadFinished(IntPtr classId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int JITCompilationStarted(IntPtr functionId, bool fIsSafeToBlock) => CorProfilerHResults.S_OK;
    public virtual int JITCompilationFinished(IntPtr functionId, int hrStatus, bool fIsSafeToBlock) => CorProfilerHResults.S_OK;
    public virtual int JITCachedFunctionSearchStarted(IntPtr functionId, out bool pbUseCachedFunction)
    {
        pbUseCachedFunction = false;
        return CorProfilerHResults.S_OK;
    }
    public virtual int JITCachedFunctionSearchFinished(IntPtr functionId, int hrStatus) => CorProfilerHResults.S_OK;
    public virtual int JITFunctionPitched(IntPtr functionId) => CorProfilerHResults.S_OK;
    public virtual int JITInlining(IntPtr callerId, IntPtr calleeId, out bool pfShouldInline)
    {
        pfShouldInline = true;
        return CorProfilerHResults.S_OK;
    }
}
