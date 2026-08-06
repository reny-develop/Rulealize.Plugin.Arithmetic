// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Plugins;

namespace Rulealize.Plugin.Arithmetic
{
    /// <summary>
    /// Arithmetic over the <c>math</c> namespace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A good illustration of why the standard vocabulary is cut so finely. Othello uses
    /// exactly one operation from here, to count consecutive passes. Folded into an omnibus
    /// plugin that single use would be invisible; as a separate entry in a rule set's
    /// <c>requires</c> list it says something true and useful — this rule set counts.
    /// </para>
    /// <para>
    /// Counting the elements of a sequence is not here. That is <c>seq.count</c>, and it
    /// belongs to the plugin that owns sequences.
    /// </para>
    /// </remarks>
    public sealed class ArithmeticPlugin : IRulealizePlugin
    {
        /// <inheritdoc />
        public PluginManifest Manifest { get; } =
            new("Rulealize.Plugin.Arithmetic", new Version(1, 0, 0), "math");

        /// <inheritdoc />
        public void Register(IPluginRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);

            registry.AddExpression("add", FoldNode.BuildAdd);
            registry.AddExpression("mul", FoldNode.BuildMultiply);
            registry.AddExpression("min", ExtremumNode.BuildMinimum);
            registry.AddExpression("max", ExtremumNode.BuildMaximum);
            registry.AddExpression("sub", BinaryArithmeticNode.BuildSubtract);
            registry.AddExpression("div", BinaryArithmeticNode.BuildDivide);
            registry.AddExpression("mod", BinaryArithmeticNode.BuildModulo);
            registry.AddExpression("abs", AbsoluteNode.Build);
        }
    }
}
