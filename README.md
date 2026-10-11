# Rulealize.Plugin.Arithmetic

Arithmetic for [Rulealize](https://github.com/reny-develop/Rulealize) rule sets.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Arithmetic` |
| Namespace | `math` |
| Reserved prefix | none |
| Depends on | `Rulealize.Abstraction` |
| Specification | [doc/specification.md](doc/specification.md) |

`math.add`, `sub`, `mul`, `div`, `mod`, `min`, `max`, `abs`, `round`. Counting the elements of a
sequence is not here; that is `seq.count`, and it belongs to the plugin that owns sequences.

Reversi uses exactly one operation from this plugin — incrementing a pass counter — which
makes it the clearest illustration of why the standard vocabulary is cut so finely. Bundled
into an omnibus plugin, that single use would be invisible. As its own line in a rule set's
`requires` list it states something true: this rule set counts.

Two things the specification settles that are worth knowing before you read it. Numbers are
`decimal`, not binary floating point, so `0.1 + 0.2 != 0.3` cannot decide a rule's outcome —
and `math.div` is therefore real division, `7 / 2` being `3.5`. And every operand must be a
number: null is an evaluation error rather than a zero, which is the mirror image of
`cmp.eq` accepting null happily.

## Building

`dotnet build`. `Rulealize.Abstraction` restores from nuget.org like any other package, so
this repository builds on its own.

`dotnet test` runs the vocabulary's own tests, in `test/`: each expression evaluated by the
Rulealize runtime as the projection of a rule set that holds nothing else. The runtime comes
from nuget.org; the plugin under test is this repository's, handed to the runtime as an
instance.

## License

Apache-2.0.
