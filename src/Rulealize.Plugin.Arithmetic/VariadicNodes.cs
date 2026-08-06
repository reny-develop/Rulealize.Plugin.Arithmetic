// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using Rulealize.Abstraction;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Nodes;
using Rulealize.Abstraction.Values;

namespace Rulealize.Plugin.Arithmetic
{
    /// <summary>Sum or product of the operands in <c>of</c>.</summary>
    /// <remarks>
    /// <para>
    /// Both are associative and commutative, so a variadic form is unambiguous and reads
    /// better than nested pairs. The empty case is the operation's identity: zero for
    /// addition, one for multiplication.
    /// </para>
    /// <para>
    /// Every operand is evaluated, even a product that has already reached zero. Nodes are
    /// pure, so nothing is observable but the cost, and a rule stated as "evaluates all of
    /// them" is easier to reason about than one with a hidden exit.
    /// </para>
    /// </remarks>
    internal sealed class FoldNode(bool multiply, ImmutableArray<ExpressionNode> operands) : ExpressionNode
    {
        public static ExpressionNode BuildAdd(INodeBuildContext context) =>
            new FoldNode(false, context.RequireExpressionArray("of"));

        public static ExpressionNode BuildMultiply(INodeBuildContext context) =>
            new FoldNode(true, context.RequireExpressionArray("of"));

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            string origin = multiply ? "math.mul" : "math.add";
            decimal accumulator = multiply ? 1m : 0m;

            for (int i = 0; i < operands.Length; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                decimal operand = operands[i].Evaluate(context).AsNumber($"{origin}.of[{i}]");
                try
                {
                    accumulator = multiply ? accumulator * operand : accumulator + operand;
                }
                catch (OverflowException exception)
                {
                    throw new RuleEvaluationException(origin, "The result is out of range.", exception);
                }
            }

            return RuleValue.Number(accumulator);
        }
    }

    /// <summary>Smallest or largest of the operands in <c>of</c>.</summary>
    /// <remarks>
    /// An empty list is an evaluation error rather than an identity, because the value
    /// model has no infinities to return. This is where the analogy with <c>math.add</c>
    /// stops.
    /// </remarks>
    internal sealed class ExtremumNode(bool maximum, ImmutableArray<ExpressionNode> operands) : ExpressionNode
    {
        public static ExpressionNode BuildMinimum(INodeBuildContext context) =>
            new ExtremumNode(false, context.RequireExpressionArray("of"));

        public static ExpressionNode BuildMaximum(INodeBuildContext context) =>
            new ExtremumNode(true, context.RequireExpressionArray("of"));

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            string origin = maximum ? "math.max" : "math.min";
            if (operands.Length == 0)
            {
                throw new RuleEvaluationException(origin, "There is no minimum or maximum of no operands.");
            }

            decimal result = operands[0].Evaluate(context).AsNumber($"{origin}.of[0]");
            for (int i = 1; i < operands.Length; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                decimal operand = operands[i].Evaluate(context).AsNumber($"{origin}.of[{i}]");
                if (maximum ? operand > result : operand < result)
                {
                    result = operand;
                }
            }

            return RuleValue.Number(result);
        }
    }

    /// <summary>Absolute value of <c>value</c>.</summary>
    internal sealed class AbsoluteNode(ExpressionNode operand) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context) =>
            new AbsoluteNode(context.RequireExpression("value"));

        public override RuleValue Evaluate(IEvaluationContext context) =>
            RuleValue.Number(Math.Abs(operand.Evaluate(context).AsNumber("math.abs.value")));
    }
}
