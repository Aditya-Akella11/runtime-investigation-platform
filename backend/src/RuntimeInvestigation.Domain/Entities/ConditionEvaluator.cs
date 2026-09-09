namespace RuntimeInvestigation.Domain.Entities;

/// <summary>
/// Evaluates simple string conditions against method argument arrays.
/// Supported syntax:
///   args[N] > value  |  args[N] >= value  |  args[N] < value
///   args[N] <= value |  args[N] == value  |  args[N] != value
///   args[N] == null  |  args[N] != null
/// </summary>
public static class ConditionEvaluator
{
    private static readonly string[] Operators = { ">=", "<=", "!=", "==", ">", "<" };

    public static bool Evaluate(string? condition, object?[] args)
    {
        if (string.IsNullOrWhiteSpace(condition)) return true;

        condition = condition.Trim();
        string? op = null;
        int opIndex = -1;

        foreach (var candidate in Operators)
        {
            int idx = condition.IndexOf(candidate, StringComparison.Ordinal);
            if (idx > 0) { op = candidate; opIndex = idx; break; }
        }

        if (op is null)
            throw new ArgumentException($"Unsupported condition expression: '{condition}'", nameof(condition));

        var left  = condition[..opIndex].Trim();
        var right = condition[(opIndex + op.Length)..].Trim();

        if (!TryParseArgRef(left, out int argIndex))
            throw new ArgumentException($"Left side must be args[N], got '{left}'", nameof(condition));

        if (argIndex < 0 || argIndex >= args.Length) return false;

        var argValue = args[argIndex];

        // null comparisons
        if (right.Equals("null", StringComparison.OrdinalIgnoreCase))
            return op == "==" ? argValue is null : argValue is not null;

        if (argValue is null) return false;

        // numeric comparison
        if (double.TryParse(right, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double rhsNum)
            && TryToDouble(argValue, out double lhsNum))
        {
            return op switch
            {
                ">"  => lhsNum > rhsNum,
                ">=" => lhsNum >= rhsNum,
                "<"  => lhsNum < rhsNum,
                "<=" => lhsNum <= rhsNum,
                "==" => Math.Abs(lhsNum - rhsNum) < 1e-10,
                "!=" => Math.Abs(lhsNum - rhsNum) >= 1e-10,
                _    => throw new ArgumentException($"Unknown operator: {op}")
            };
        }

        // string comparison
        var lhsStr = argValue.ToString() ?? string.Empty;
        var rhsStr = right.Trim('"', '\'');
        int cmp = string.Compare(lhsStr, rhsStr, StringComparison.Ordinal);
        return op switch
        {
            "==" => cmp == 0,
            "!=" => cmp != 0,
            ">"  => cmp > 0,
            ">=" => cmp >= 0,
            "<"  => cmp < 0,
            "<=" => cmp <= 0,
            _    => throw new ArgumentException($"Unknown operator: {op}")
        };
    }

    private static bool TryParseArgRef(string token, out int index)
    {
        index = -1;
        if (!token.StartsWith("args[", StringComparison.OrdinalIgnoreCase) || !token.EndsWith("]"))
            return false;
        return int.TryParse(token["args[".Length..^1], out index);
    }

    private static bool TryToDouble(object value, out double result)
    {
        result = 0;
        try { result = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture); return true; }
        catch { return false; }
    }
}
