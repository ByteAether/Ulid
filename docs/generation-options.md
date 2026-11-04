# Generation options

`Ulid.New()` works without configuration. Pass a `Ulid.GenerationOptions` instance, or set `Ulid.DefaultGenerationOptions`, to change the monotonicity or the source of randomness.

## Monotonicity

`Monotonicity` defines what happens when more than one ULID is generated within the same millisecond.

| Value | Behavior | Use for |
| --- | --- | --- |
| `NonMonotonic` | Fully random 80-bit payload; no state is kept. | Cases where order within a millisecond does not matter. |
| `MonotonicIncrement` (default) | Adds 1 to the previous random payload. | Strict ordering with the least overhead. |
| `MonotonicRandom1Byte` to `MonotonicRandom4Byte` | Adds a random increment to the previous random payload. The increment is 1 to 4 random bytes (each `00` to `FF`) plus 1, so it ranges from 1 up to 256, 65 536, 16 777 216, or 4 294 967 296. | Strict ordering with less predictable IDs. |

When the 80-bit random payload overflows within a millisecond, the timestamp advances by one millisecond. This avoids an exception unless the sequence is already at `Ulid.MaxValue`; incrementing that value throws `OverflowException`. See [design notes](design-notes.md#overflow-handling).

## Monotonicity state

The last generated timestamp and random payload belong to the `GenerationOptions` instance that produced them.

- Reusing one instance across calls continues a single monotonic sequence across calls and threads. This is the recommended setup. Access to the state is serialized by a low-latency test-and-test-and-set (TTAS) lock with compare-and-swap (CAS) acquisition and adaptive back-off.
- If a requested timestamp is not later than the previous one, the generator increments the previous ULID. This also preserves increasing order when the clock moves backwards.
- Passing a new instance on each call gives each call its own state. Monotonic ordering between those calls is not tracked, and there is no lock contention.
- A copy made with a `with` expression (for example `options with { Monotonicity = ... }`) gets its own fresh state. It does not continue or share the sequence of the instance it was copied from.

## Example: random increments

Keep the options in a `static readonly` field so every call continues the same sequence:

```csharp
using ByteAether.Ulid;
using static ByteAether.Ulid.Ulid.GenerationOptions;

public static class Ids
{
	private static readonly Ulid.GenerationOptions _options = new()
	{
		Monotonicity = MonotonicityOptions.MonotonicRandom2Byte
	};

	public static Ulid Next() => Ulid.New(_options);
}
```

## Example: application-wide defaults

Set the defaults once during application startup. Later calls to `Ulid.New()` without a `GenerationOptions` argument use them.

```csharp
using ByteAether.Ulid;
using static ByteAether.Ulid.Ulid.GenerationOptions;

Ulid.DefaultGenerationOptions = new()
{
	Monotonicity = MonotonicityOptions.MonotonicRandom2Byte,
	InitialRandomSource = new CryptographicallySecureRandomProvider(),
	IncrementRandomSource = new PseudoRandomProvider()
};

var ulid = Ulid.New();
```

The sources in this example are the defaults, so only `Monotonicity` differs from an unconfigured library. Without `Monotonicity`, the default is `MonotonicIncrement`, which adds 1 and is fully predictable within a millisecond.

> [!WARNING]
> `PseudoRandomProvider` is faster than `CryptographicallySecureRandomProvider`, but its output is predictable. Someone who has seen other IDs from the same source can guess later increments. A ULID generated with pseudo-random initial bytes is also predictable. Use `CryptographicallySecureRandomProvider` for `IncrementRandomSource` when increments must not be guessable, and never use `PseudoRandomProvider` for `InitialRandomSource` when ULIDs act as secrets or must not be enumerable.

## Random sources

`InitialRandomSource` supplies the random bytes of a ULID. The default is `CryptographicallySecureRandomProvider`, which uses `System.Security.Cryptography.RandomNumberGenerator`.

`IncrementRandomSource` supplies the random increments for `MonotonicRandom1Byte` to `MonotonicRandom4Byte`. The default is `PseudoRandomProvider`, which is based on `System.Random`. It is faster than a cryptographic source, and its output is predictable.

To use another source, implement `IRandomProvider`.
