// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Nodes;
using Rulealize.Abstraction.Values;

namespace Rulealize.Plugin.Arithmetic
{
    /// <summary>Which of the non-commutative operations a node performs.</summary>
    internal enum BinaryOperation
    {
        Subtract,
        Divide,
        Modulo
    }

    /// <summary>Subtraction, division and remainder, written as <c>left</c> and <c>right</c>.</summary>
    /// <remarks>
    /// <para>
    /// These are not variadic. A list <c>[a, b, c]</c> under a non-commutative operation
    /// does not say which way it folds, whereas <c>left</c> and <c>right</c> leave nothing
    /// to guess.
    /// </para>
    /// <para>
    /// Division is real division: <c>7 / 2</c> is <c>3.5</c>, not <c>3</c>. The value model
    /// has one numeric kind and no integer type to truncate towards. Remainder takes the
    /// sign of the dividend, as it does in C#.
    /// </para>
    /// </remarks>
    internal sealed class BinaryArithmeticNode(BinaryOperation operation, ExpressionNode left, ExpressionNode right)
        : ExpressionNode
    {
        public static ExpressionNode BuildSubtract(INodeBuildContext context) =>
            Build(context, BinaryOperation.Subtract);

        public static ExpressionNode BuildDivide(INodeBuildContext context) =>
            Build(context, BinaryOperation.Divide);

        public static ExpressionNode BuildModulo(INodeBuildContext context) =>
            Build(context, BinaryOperation.Modulo);

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            string origin = $"math.{OperationName(operation)}";
            decimal leftValue = left.Evaluate(context).AsNumber($"{origin}.left");
            decimal rightValue = right.Evaluate(context).AsNumber($"{origin}.right");

            if (operation != BinaryOperation.Subtract && rightValue == 0m)
            {
                throw new RuleEvaluationException(origin, "The right operand is zero.");
            }

            try
            {
                return RuleValue.Number(operation switch
                {
                    BinaryOperation.Subtract => leftValue - rightValue,
                    BinaryOperation.Divide => leftValue / rightValue,
                    _ => leftValue % rightValue
                });
            }
            catch (OverflowException exception)
            {
                throw new RuleEvaluationException(origin, "The result is out of range.", exception);
            }
        }

        private static ExpressionNode Build(INodeBuildContext context, BinaryOperation operation) =>
            new BinaryArithmeticNode(operation, context.RequireExpression("left"), context.RequireExpression("right"));

        private static string OperationName(BinaryOperation operation) => operation switch
        {
            BinaryOperation.Subtract => "sub",
            BinaryOperation.Divide => "div",
            _ => "mod"
        };
    }
}
