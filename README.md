# Rulealize.Plugin.Arithmetic

Arithmetic for [Rulealize](https://github.com/reny-develop/Rulealize) rule sets.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Arithmetic` |
| Namespace | `math` |
| Reserved prefix | none |
| Depends on | `Rulealize.Abstraction` |

Othello uses exactly one operation from this plugin — incrementing a pass counter — which
makes it the clearest illustration of why the standard vocabulary is cut so finely. Bundled
into an omnibus plugin, that single use would be invisible. As its own line in a rule set's
`requires` list it states something true: this rule set counts.

Counting the elements of a sequence is not here; that is `seq.count`, and it belongs to the
plugin that owns sequences.

## Operations

| Operation | Shape |
| --- | --- |
| `math.add` / `math.mul` | `{ "op": "math.add", "of": [ … ] }` |
| `math.min` / `math.max` | `{ "op": "math.min", "of": [ … ] }` |
| `math.sub` / `math.div` / `math.mod` | `{ "op": "math.sub", "left": …, "right": … }` |
| `math.abs` | `{ "op": "math.abs", "value": … }` |

Empty lists: `math.add` is `0` and `math.mul` is `1`, each the identity of its operation.
`math.min` and `math.max` of nothing is an evaluation error — the value model has no
infinities to hand back.

The commutative operations are variadic; the others are not. A list under a
non-commutative operation does not say which way it folds, whereas `left` and `right`
leave nothing to guess.

## Numbers are decimal

The value model's `Number` is backed by `decimal`, not by a binary floating point type.
Rule descriptions deal in quantities written in decimal — piece counts, coordinates,
scores, probabilities — and `0.1 + 0.2 != 0.3` must not be able to decide a rule's outcome.
A result outside the range of `decimal` is an evaluation error rather than a silent
rounding.

Two consequences worth knowing:

- `math.div` is real division. `7 / 2` is `3.5`. There is no integer kind to truncate
  towards, and no `math.floor` yet to do it explicitly.
- `math.mod` takes the sign of the dividend, as `%` does in C#: `-7 mod 3` is `-1`. A rule
  that needs the always-positive mathematical remainder has to correct for it.

## Null and text are refused

Every operand must be a number. Null is an evaluation error, and so is text — `"1" + 1`
does not work.

This is the mirror image of `cmp.eq`, which accepts null happily, and the asymmetry is
deliberate. "Is the absent value equal to black" has an obvious answer; "what is the
absent value plus one" does not. Treating null as zero would let an empty board square be
counted as a score of nothing, which is exactly the sort of mistake that survives testing.
Flatten the null first, with `cmp.coalesce`.

## Building

`Rulealize.Abstraction` is not on nuget.org yet, so `NuGet.config` points at a folder
feed. Produce it from the abstraction repository first:

```
dotnet pack path\to\Rulealize.Abstraction\src\Rulealize.Abstraction -c Release -o path\to\LocalNuGet
```

with `LocalNuGet` a sibling of this repository. Then `dotnet build`.

## License

Apache-2.0.
