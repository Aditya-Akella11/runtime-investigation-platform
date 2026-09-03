using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public static class IlWrapperSpike
{
    private static readonly System.Threading.AsyncLocal<Action<string, object?[]>?> _onMethodEntry = new();
    private static readonly System.Threading.AsyncLocal<Action<string, object?>?> _onMethodExit = new();

    public static Action<string, object?[]>? OnMethodEntry
    {
        get => _onMethodEntry.Value;
        set => _onMethodEntry.Value = value;
    }

    public static Action<string, object?>? OnMethodExit
    {
        get => _onMethodExit.Value;
        set => _onMethodExit.Value = value;
    }

    public static void LogEntry(string methodName, object?[] args)
    {
        OnMethodEntry?.Invoke(methodName, args);
    }

    public static void LogExit(string methodName, object? result)
    {
        OnMethodExit?.Invoke(methodName, result);
    }

    public static Delegate CreateWrapper(MethodInfo targetMethod)
    {
        var parameterTypes = GetParameterTypes(targetMethod);
        var returnType = targetMethod.ReturnType;

        var dynamicMethod = new DynamicMethod(
            $"Wrapped_{targetMethod.Name}_{Guid.NewGuid():N}",
            returnType,
            parameterTypes,
            targetMethod.Module,
            skipVisibility: true);

        var il = dynamicMethod.GetILGenerator();

        // 1. Log target method entry
        // Method name string representation
        il.Emit(OpCodes.Ldstr, $"{targetMethod.DeclaringType?.FullName ?? "Global"}.{targetMethod.Name}");

        // Create object[] array to hold the arguments for LogEntry
        int paramCount = parameterTypes.Length;
        il.Emit(OpCodes.Ldc_I4, paramCount);
        il.Emit(OpCodes.Newarr, typeof(object));

        for (int i = 0; i < paramCount; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldarg, i);

            // Box value types so they can be placed in object[]
            var paramType = parameterTypes[i];
            if (paramType.IsValueType)
            {
                il.Emit(OpCodes.Box, paramType);
            }

            il.Emit(OpCodes.Stelem_Ref);
        }

        // Call LogEntry(string methodName, object?[] args)
        var logEntryMethod = typeof(IlWrapperSpike).GetMethod(nameof(LogEntry), BindingFlags.Public | BindingFlags.Static);
        il.Emit(OpCodes.Call, logEntryMethod!);

        // 2. Load arguments for forwarding to the targetMethod
        for (int i = 0; i < paramCount; i++)
        {
            il.Emit(OpCodes.Ldarg, i);
        }

        // Call targetMethod
        il.Emit(OpCodes.Call, targetMethod);

        // 3. Log target method exit
        if (returnType == typeof(void))
        {
            il.Emit(OpCodes.Ldstr, $"{targetMethod.DeclaringType?.FullName ?? "Global"}.{targetMethod.Name}");
            il.Emit(OpCodes.Ldnull);
            var logExitMethod = typeof(IlWrapperSpike).GetMethod(nameof(LogExit), BindingFlags.Public | BindingFlags.Static);
            il.Emit(OpCodes.Call, logExitMethod!);
        }
        else
        {
            var retLocal = il.DeclareLocal(returnType);
            il.Emit(OpCodes.Stloc, retLocal);

            il.Emit(OpCodes.Ldstr, $"{targetMethod.DeclaringType?.FullName ?? "Global"}.{targetMethod.Name}");
            il.Emit(OpCodes.Ldloc, retLocal);
            if (returnType.IsValueType)
            {
                il.Emit(OpCodes.Box, returnType);
            }
            var logExitMethod = typeof(IlWrapperSpike).GetMethod(nameof(LogExit), BindingFlags.Public | BindingFlags.Static);
            il.Emit(OpCodes.Call, logExitMethod!);

            il.Emit(OpCodes.Ldloc, retLocal);
        }

        il.Emit(OpCodes.Ret);

        Type delegateType;
        if (returnType == typeof(void))
        {
            delegateType = System.Linq.Expressions.Expression.GetActionType(parameterTypes);
        }
        else
        {
            var delegateTypes = new Type[parameterTypes.Length + 1];
            Array.Copy(parameterTypes, delegateTypes, parameterTypes.Length);
            delegateTypes[^1] = returnType;
            delegateType = System.Linq.Expressions.Expression.GetFuncType(delegateTypes);
        }

        return dynamicMethod.CreateDelegate(delegateType);
    }

    private static Type[] GetParameterTypes(MethodInfo targetMethod)
    {
        var parameters = targetMethod.GetParameters();
        bool isStatic = targetMethod.IsStatic;
        int offset = isStatic ? 0 : 1;
        var types = new Type[parameters.Length + offset];

        if (!isStatic)
        {
            types[0] = targetMethod.DeclaringType ?? typeof(object);
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            types[i + offset] = parameters[i].ParameterType;
        }

        return types;
    }
}
