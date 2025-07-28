using static ByteAether.Ulid.Ulid;

namespace ByteAether.Ulid.Tests
{
    public class GenerationOptionsTests
    {
        [Fact]
        public void DefaultConstructor_ShouldHaveCorrectDefaultValues()
        {
            // Act
            var options = new GenerationOptions();

            // Assert
            Assert.Equal(GenerationOptions.MonotonicityOptions.MonotonicIncrement, options.Monotonicity);
            Assert.Equal(GenerationOptions.RandomSourceOptions.CryptographicallySecure, options.InitialRandomSource);
            Assert.Equal(GenerationOptions.RandomSourceOptions.PseudoRandom, options.IncrementRandomSource);
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

        [Theory]
        [CombinatorialData]
        public void InitialRandomSource_SetValidOption_ShouldSucceed(GenerationOptions.RandomSourceOptions randomSource)
        {
            // Act
            var options = new GenerationOptions { InitialRandomSource = randomSource };

            // Assert
            Assert.Equal(randomSource, options.InitialRandomSource);
        }

        [Fact]
        public void InitialRandomSource_SetInvalidOption_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var invalidSource = (GenerationOptions.RandomSourceOptions)99;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GenerationOptions { InitialRandomSource = invalidSource });
            Assert.Contains("Invalid initial random source option.", ex.Message);
        }

        [Theory]
        [CombinatorialData]
        public void IncrementRandomSource_SetValidOption_ShouldSucceed(GenerationOptions.RandomSourceOptions randomSource)
        {
            // Act
            var options = new GenerationOptions { IncrementRandomSource = randomSource };

            // Assert
            Assert.Equal(randomSource, options.IncrementRandomSource);
        }

        [Fact]
        public void IncrementRandomSource_SetInvalidOption_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var invalidSource = (GenerationOptions.RandomSourceOptions)99;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GenerationOptions { IncrementRandomSource = invalidSource });
            Assert.Contains("Invalid increment random source option.", ex.Message);
        }
    }
}