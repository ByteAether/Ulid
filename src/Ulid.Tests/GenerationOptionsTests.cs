using static ByteAether.Ulid.Ulid;

namespace ByteAether.Ulid.Tests;

public class GenerationOptionsTests
{
	[Fact]
	public void DefaultConstructor_ShouldHaveCorrectDefaultValues()
	{
		// Act
		var options = new GenerationOptions();

		// Assert
		Assert.Equal(GenerationOptions.MonotonicityOptions.MonotonicIncrement, options.Monotonicity);
		Assert.Equal(typeof(CryptographicallySecureRandomProvider), options.InitialRandomSource.GetType());
		Assert.Equal(typeof(PseudoRandomProvider), options.IncrementRandomSource.GetType());
	}

	[Theory]
	[CombinatorialData]
	public void Monotonicity_SetValidOption_ShouldSucceed(GenerationOptions.MonotonicityOptions monotonicity)
	{
		// Act
		var options = new GenerationOptions { Monotonicity = monotonicity };

		// Assert
		Assert.Equal(monotonicity, options.Monotonicity);
	}

	[Fact]
	public void Monotonicity_SetInvalidOption_ShouldThrowArgumentOutOfRangeException()
	{
		// Arrange
		var invalidMonotonicity = (GenerationOptions.MonotonicityOptions)99;

		// Act & Assert
		var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GenerationOptions { Monotonicity = invalidMonotonicity });
		Assert.Contains("Invalid monotonicity option.", ex.Message);
	}

	[Fact]
	public void Monotonicity_EveryDefinedOption_ShouldBeAccepted()
	{
		// Fails when an option is added to the enum without updating the validation
		foreach (var monotonicity in Enum.GetValues<GenerationOptions.MonotonicityOptions>())
		{
			// Act
			var options = new GenerationOptions { Monotonicity = monotonicity };

			// Assert
			Assert.Equal(monotonicity, options.Monotonicity);
		}
	}

	[Fact]
	public void Monotonicity_ValuesNextToDefinedOptions_ShouldThrow()
	{
		// Arrange
		var definedValues = Enum.GetValues<GenerationOptions.MonotonicityOptions>().Select(x => (int)x).ToArray();
		var belowMinimum = (GenerationOptions.MonotonicityOptions)(definedValues.Min() - 1);
		var aboveMaximum = (GenerationOptions.MonotonicityOptions)(definedValues.Max() + 1);

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => new GenerationOptions { Monotonicity = belowMinimum });
		Assert.Throws<ArgumentOutOfRangeException>(() => new GenerationOptions { Monotonicity = aboveMaximum });
	}

	[Fact]
	public void With_ShouldCopyConfiguration()
	{
		// Arrange
		var initialRandomSource = new PseudoRandomProvider();
		var incrementRandomSource = new CryptographicallySecureRandomProvider();
		var options = new GenerationOptions
		{
			Monotonicity = GenerationOptions.MonotonicityOptions.MonotonicRandom3Byte,
			InitialRandomSource = initialRandomSource,
			IncrementRandomSource = incrementRandomSource,
		};

		// Act
		var copy = options with { };

		// Assert
		Assert.Equal(options.Monotonicity, copy.Monotonicity);
		Assert.Same(options.InitialRandomSource, copy.InitialRandomSource);
		Assert.Same(options.IncrementRandomSource, copy.IncrementRandomSource);
	}

	[Fact]
	public void With_ShouldNotShareMonotonicityState()
	{
		// Arrange
		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var options = new GenerationOptions
		{
			InitialRandomSource = new FixedRandomProvider(),
		};
		var copy = options with { };

		// Act
		var first = New(timestamp, options);
		var fromCopy = New(timestamp, copy);
		var second = New(timestamp, options);

		// Assert
		Assert.Equal(first, fromCopy); // The copy starts its own sequence
		Assert.True(second > first);
	}

	private sealed class FixedRandomProvider : IRandomProvider
	{
		public void GetBytes(Span<byte> buffer) => buffer.Fill(0x42);
	}
}