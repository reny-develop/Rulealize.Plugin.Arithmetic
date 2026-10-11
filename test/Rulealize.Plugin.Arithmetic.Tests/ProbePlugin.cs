// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Text.Json;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Plugin;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Arithmetic.Tests;

/// <summary>The one schema node a rule set needs before it can hold a projection to evaluate: a field that holds nothing.</summary>
/// <remarks>
/// Every rule set declares at least one state field, and a schema node is some vocabulary's to
/// provide. Borrowing TypeSchema for it would pin a version of another repository into this one's
/// tests for a field nothing reads, so the tests carry the smallest schema there is instead.
/// </remarks>
internal sealed class ProbePlugin : IRulealizePlugin
{
    /// <summary>The identifier a rule set requires this by.</summary>
    public const string Id = "Probe";

    /// <inheritdoc />
    public PluginManifest Manifest { get; } = new(Id, new Version(1, 0, 0), "probe");

    /// <inheritdoc />
    public void Register(IPluginRegistry registry) => registry.AddSchema("nothing", _ => new NothingSchemaNode());

    /// <summary>A field whose only value is null.</summary>
    private sealed class NothingSchemaNode : SchemaNode
    {
        public override bool IsNullable => true;

        public override void Validate(RuleValue value, ISchemaValidationSink sink)
        {
            if (!value.IsNull)
            {
                sink.Violation("Expected null.");
            }
        }

        public override RuleValue ReadJson(JsonElement element, ISchemaValidationSink sink)
        {
            if (element.ValueKind != JsonValueKind.Null)
            {
                sink.Violation("Expected null.");
            }

            return RuleValue.Null;
        }

        public override void WriteJson(Utf8JsonWriter writer, RuleValue value) => writer.WriteNullValue();
    }
}
