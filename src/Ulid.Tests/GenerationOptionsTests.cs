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
}