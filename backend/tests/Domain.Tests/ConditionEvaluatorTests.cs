using System;
using RuntimeInvestigation.Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class ConditionEvaluatorTests
{
    [Fact]
    public void Evaluate_NullOrWhitespaceCondition_ReturnsTrue()
    {
        var args = new object?[] { 42 };
        Assert.True(ConditionEvaluator.Evaluate(null, args));
        Assert.True(ConditionEvaluator.Evaluate("", args));
        Assert.True(ConditionEvaluator.Evaluate("   ", args));
    }

    [Fact]
    public void Evaluate_GreaterThan_ReturnsTrueWhenGreater()
    {
        var args = new object?[] { 150 };
        Assert.True(ConditionEvaluator.Evaluate("args[0] > 100", args));
    }

    [Fact]
    public void Evaluate_GreaterThan_ReturnsFalseWhenLessOrEqual()
    {
        var args = new object?[] { 50 };
        Assert.False(ConditionEvaluator.Evaluate("args[0] > 100", args));

        var argsEqual = new object?[] { 100 };
        Assert.False(ConditionEvaluator.Evaluate("args[0] > 100", argsEqual));
    }

    [Fact]
    public void Evaluate_EqualNumeric_ReturnsTrueWhenEqual()
    {
        var args = new object?[] { 200 };
        Assert.True(ConditionEvaluator.Evaluate("args[0] == 200", args));
        Assert.False(ConditionEvaluator.Evaluate("args[0] == 500", args));
    }

    [Fact]
    public void Evaluate_NotEqualNumeric_ReturnsTrueWhenDifferent()
    {
        var args = new object?[] { 404 };
        Assert.True(ConditionEvaluator.Evaluate("args[0] != 200", args));
        Assert.False(ConditionEvaluator.Evaluate("args[0] != 404", args));
    }

    [Fact]
    public void Evaluate_NullComparison_ReturnsExpected()
    {
        var argsWithNull = new object?[] { null };
        var argsWithVal = new object?[] { "active" };

        Assert.True(ConditionEvaluator.Evaluate("args[0] == null", argsWithNull));
        Assert.False(ConditionEvaluator.Evaluate("args[0] != null", argsWithNull));

        Assert.False(ConditionEvaluator.Evaluate("args[0] == null", argsWithVal));
        Assert.True(ConditionEvaluator.Evaluate("args[0] != null", argsWithVal));
    }

    [Fact]
    public void Evaluate_StringComparison_ReturnsExpected()
    {
        var args = new object?[] { "production" };
        Assert.True(ConditionEvaluator.Evaluate("args[0] == production", args));
        Assert.False(ConditionEvaluator.Evaluate("args[0] == staging", args));
    }

    [Fact]
    public void Evaluate_InvalidExpression_ThrowsArgumentException()
    {
        var args = new object?[] { 123 };
        Assert.Throws<ArgumentException>(() => ConditionEvaluator.Evaluate("invalid_expression", args));
        Assert.Throws<ArgumentException>(() => ConditionEvaluator.Evaluate("foo == 123", args));
    }
}
