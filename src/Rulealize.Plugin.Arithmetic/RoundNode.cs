// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using Rulealize.Abstraction;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Arithmetic
{
    /// <summary><c>value</c> rounded to <c>scale</c> decimal places, the way <c>mode</c> says.</summary>
    /// <remarks>
    /// <para>
    /// The mode has no default. Half away from zero, half to even and towards zero are each the
    /// right answer somewhere — a shop's receipt, a bank's ledger, a tax that is never rounded in
    /// the payer's favour — and which one a rule set means is a rule of its domain. A default
    /// would be a rule nobody wrote, so the document has to say it.
    /// </para>
    /// <para>
    /// The names say what happens to a negative number as well as a positive one, which
    /// "half up" and "down" do not: rounding −2.5 "up" is −2 to some readers and −3 to others.
    /// </para>
    /// </remarks>
    internal sealed class RoundNode(ExpressionNode operand, int scale, MidpointRounding mode) : ExpressionNode
    {
        /// <summary>The most decimal places <see cref="decimal"/> holds.</summary>
        private const int MostScale = 28;

        /// <summary>Each mode a document may name, and what it is in .NET.</summary>
        private static readonly IReadOnlyList<(string Name, MidpointRounding Mode)> Modes =
        [
            ("halfAwayFromZero", MidpointRounding.AwayFromZero),
            ("halfEven", MidpointRounding.ToEven),
            ("towardZero", MidpointRounding.ToZero),
            ("floor", MidpointRounding.ToNegativeInfinity),
            ("ceiling", MidpointRounding.ToPositiveInfinity),
        ];

        public static ExpressionNode Build(INodeBuildContext context)
        {
            int scale = context.OptionalInt32("scale", 0);
            if (scale is < 0 or > MostScale)
            {
                throw context.Error("scale", $"must be from 0 to {MostScale}, but is {scale.ToString(CultureInfo.InvariantCulture)}.");
            }

            string named = context.RequireString("mode");
            (string Name, MidpointRounding Mode) mode = Modes.FirstOrDefault(each => each.Name == named);
            if (mode.Name is null)
            {
                throw context.Error("mode", $"is one of {string.Join(", ", Modes.Select(each => each.Name))}, and '{named}' is none of them.");
            }

            return new RoundNode(context.RequireExpression("value"), scale, mode.Mode);
        }

        public override RuleValue Evaluate(IEvaluationContext context) =>
            RuleValue.Number(decimal.Round(operand.Evaluate(context).AsNumber("math.round.value"), scale, mode));
    }
}
