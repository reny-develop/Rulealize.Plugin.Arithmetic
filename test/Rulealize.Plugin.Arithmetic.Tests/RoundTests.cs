// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Text.Json;
using Rulealize.Abstraction;

namespace Rulealize.Plugin.Arithmetic.Tests;

/// <summary>Rounding, which a rule set states the way of, since there is no one right way.</summary>
/// <remarks>
/// Each expression is a projection of a rule set that holds nothing else, read the way a host reads
/// one, so what is tested is the operation as a rule set meets it and nothing stands in for the runtime.
/// </remarks>
public sealed class RoundTests
{
    private static readonly RuleRuntime Runtime = new RuleRuntime().AddPlugin(new ArithmeticPlugin()).AddPlugin(new ProbePlugin());

    [Theory]
    [InlineData("2.5", "halfAwayFromZero", 0, "3")]
    [InlineData("-2.5", "halfAwayFromZero", 0, "-3")]
    [InlineData("2.4", "halfAwayFromZero", 0, "2")]
    [InlineData("2.5", "halfEven", 0, "2")]
    [InlineData("3.5", "halfEven", 0, "4")]
    [InlineData("-2.5", "halfEven", 0, "-2")]
    [InlineData("2.9", "towardZero", 0, "2")]
    [InlineData("-2.9", "towardZero", 0, "-2")]
    [InlineData("2.1", "floor", 0, "2")]
    [InlineData("-2.1", "floor", 0, "-3")]
    [InlineData("2.1", "ceiling", 0, "3")]
    [InlineData("-2.9", "ceiling", 0, "-2")]
    [InlineData("12.345", "halfAwayFromZero", 2, "12.35")]
    [InlineData("12.345", "halfEven", 2, "12.34")]
    [InlineData("12.3", "floor", 2, "12.3")]
    public void AValueIsRoundedToTheScaleTheWayTheModeSays(string value, string mode, int scale, string rounded) =>
        Assert.Equal(decimal.Parse(rounded, System.Globalization.CultureInfo.InvariantCulture), Evaluated($$"""
            { "op": "math.round", "value": {{value}}, "scale": {{scale}}, "mode": "{{mode}}" }
            """));

    [Fact]
    public void TheScaleIsZeroWhereItIsNotWritten() =>
        Assert.Equal(3m, Evaluated("""{ "op": "math.round", "value": 2.5, "mode": "halfAwayFromZero" }"""));

    [Fact]
    public void TheTaxOnAnAmountIsCutOffAtTheYen() =>
        // The case that asked for the node, and the specification's example.
        Assert.Equal(123m, Evaluated("""
            { "op": "math.round", "mode": "towardZero", "value": { "op": "math.mul", "of": [1239, 0.1] } }
            """));

    [Theory]
    [InlineData("\"value\": 1", "mode")]
    [InlineData("\"value\": 1, \"mode\": \"halfUp\"", "is one of halfAwayFromZero, halfEven, towardZero, floor, ceiling, and 'halfUp' is none of them")]
    [InlineData("\"value\": 1, \"mode\": \"floor\", \"scale\": 29", "must be from 0 to 28")]
    [InlineData("\"value\": 1, \"mode\": \"floor\", \"scale\": -1", "must be from 0 to 28")]
    public void AWayOfRoundingNotStatedIsRefusedWhenTheRuleSetIsBuilt(string fields, string said) =>
        Assert.Contains(
            said,
            Assert.Throws<RuleSetBuildException>(() => Runtime.CreateContext(RuleSet($$"""{ "op": "math.round", {{fields}} }"""))).Message,
            StringComparison.Ordinal);

    [Fact]
    public void AValueThatIsNotANumberIsAFault() =>
        Assert.Equal(
            "math.round.value",
            Assert.Throws<RuleEvaluationException>(() => Evaluated("""{ "op": "math.round", "value": "2.5", "mode": "floor" }""")).Origin);

    /// <summary>What an expression evaluates to, read as the projection of a rule set that holds only it.</summary>
    private static decimal Evaluated(string expression)
    {
        RuleContext context = Runtime.CreateContext(RuleSet(expression));
        using JsonDocument value = JsonDocument.Parse(context.Project("value", context.InitialState));
        return value.RootElement.GetDecimal();
    }

    private static string RuleSet(string expression) => $$"""
        {
          "id": "probe", "version": "1.0.0",
          "requires": [ { "plugin": "Rulealize.Plugin.Arithmetic" }, { "plugin": "Probe" } ],
          "state": { "schema": { "unused": { "op": "probe.nothing" } }, "initial": { "unused": null } },
          "inputs": { "nothing": { "effects": [] } },
          "projections": { "value": {{expression}} }
        }
        """;
}
