namespace ByteAether.Ulid.Tests;

public class UlidNewTests
{
	// A lock object to synchronize tests that rely on shared static state in the Ulid class,
	// preventing race conditions and ensuring test isolation.
	private static readonly object _staticStateLock = new();

	[Fact]
	public void ToByteArray_ShouldConvertToByteArrayAndBack()
	{
		// Arrange
		var ulid = Ulid.New();

		// Act
		var byteArray = ulid.ToByteArray();
		var ulidFromBytes = Ulid.New(byteArray);

		// Assert
		Assert.Equal(16, byteArray.Length);
		Assert.Equal(ulid, ulidFromBytes);
	}

	[Theory]
	[CombinatorialData]
	public void New_ShouldGenerateUniqueUlids(Ulid.GenerationOptions.MonotonicityOptions monotonicity)
	{
		var options = new Ulid.GenerationOptions { Monotonicity = monotonicity };

		lock (_staticStateLock)
		{
			// Act
			var ulid1 = Ulid.New(options);
			var ulid2 = Ulid.New(options);

			// Assert
			Assert.NotEqual(ulid1, ulid2);
		}
	}

	[Theory]
	[CombinatorialData]
	public void New_WithDateTime_ShouldGenerateUniqueUlids(Ulid.GenerationOptions.MonotonicityOptions monotonicity)
	{
		// Arrange
		var dateTimeOffset = DateTimeOffset.UtcNow;
		var timestamp = dateTimeOffset.ToUnixTimeMilliseconds();
		var options = new Ulid.GenerationOptions { Monotonicity = monotonicity };

		lock (_staticStateLock)
		{
			Ulid.New(options); // Prime the pump to ensure the last-generated ULID state is recent.

			// Act
			var ulid1 = Ulid.New(dateTimeOffset, options);
			var ulid2 = Ulid.New(dateTimeOffset, options);

			// Assert
			Assert.NotEqual(ulid1, ulid2);

			Assert.True(ulid1.Time.ToUnixTimeMilliseconds() <= ulid2.Time.ToUnixTimeMilliseconds());
			Assert.True(timestamp <= ulid1.Time.ToUnixTimeMilliseconds());
			Assert.True(timestamp <= ulid2.Time.ToUnixTimeMilliseconds());

			if (monotonicity != Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic)
			{
				Assert.True(ulid1.AsByteSpan().SequenceCompareTo(ulid2.AsByteSpan()) < 0);
				Assert.True(ulid1 < ulid2);
			}
		}
	}

	[Theory]
	[CombinatorialData]
	public void New_WithTimestamp_ShouldGenerateUniqueUlids(Ulid.GenerationOptions.MonotonicityOptions monotonicity)
	{
		// Arrange
		var dateTimeOffset = DateTimeOffset.UtcNow;
		var timestamp = dateTimeOffset.ToUnixTimeMilliseconds();
		var options = new Ulid.GenerationOptions { Monotonicity = monotonicity };

		lock (_staticStateLock)
		{
			Ulid.New(options); // Prime the pump to ensure the last-generated ULID state is recent.

			// Act
			var ulid1 = Ulid.New(timestamp, options);
			var ulid2 = Ulid.New(timestamp, options);

			// Assert
			Assert.NotEqual(ulid1, ulid2);

			Assert.True(ulid1.Time.ToUnixTimeMilliseconds() <= ulid2.Time.ToUnixTimeMilliseconds());
			Assert.True(timestamp <= ulid1.Time.ToUnixTimeMilliseconds());
			Assert.True(timestamp <= ulid2.Time.ToUnixTimeMilliseconds());

			if (monotonicity != Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic)
			{
				Assert.True(ulid1.AsByteSpan().SequenceCompareTo(ulid2.AsByteSpan()) < 0);
				Assert.True(ulid1 < ulid2);
			}
		}
	}

	[Fact]
	public void New_WithDateTimeAndRandom_ShouldGenerateSameUlid()
	{
		// Arrange
		var dateTimeOffset = DateTimeOffset.UtcNow;
		var timestamp = dateTimeOffset.ToUnixTimeMilliseconds();
		var random = new byte[10];

		// Act
		var ulid1 = Ulid.New(dateTimeOffset, random);
		var ulid2 = Ulid.New(dateTimeOffset, random);

		// Assert
		Assert.Equal(ulid1, ulid2);

		Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
		Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());

		Assert.Equal(ulid1.Time, ulid2.Time);
		Assert.Equal(ulid1.TimeBytes.ToArray(), ulid2.TimeBytes.ToArray());

		Assert.Equal(ulid1.Random.ToArray(), ulid2.Random.ToArray());
	}

	[Fact]
	public void New_WithTimestampAndRandom_ShouldGenerateSameUlid()
	{
		// Arrange
		var dateTimeOffset = DateTimeOffset.UtcNow;
		var timestamp = dateTimeOffset.ToUnixTimeMilliseconds();
		var random = new byte[10];

		// Act
		var ulid1 = Ulid.New(timestamp, random);
		var ulid2 = Ulid.New(timestamp, random);

		// Assert
		Assert.Equal(ulid1, ulid2);

		Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
		Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());

		Assert.Equal(ulid1.Time, ulid2.Time);
		Assert.Equal(ulid1.TimeBytes.ToArray(), ulid2.TimeBytes.ToArray());

		Assert.Equal(ulid1.Random.ToArray(), ulid2.Random.ToArray());
	}

	[Theory]
	[CombinatorialData]
	public void New_WithTimestampAndMonotonicSet_ShouldGenerateUniqueUlids(
		Ulid.GenerationOptions.MonotonicityOptions? monotonicity,
		Ulid.GenerationOptions.MonotonicityOptions defaultMonotonicity
	)
	{
		// Arrange
		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		// When monotonicity is specified, it creates new options. Otherwise, null is passed to Ulid.New
		// and DefaultGenerationOptions should be used.
		var options = monotonicity.HasValue
			? Ulid.DefaultGenerationOptions with { Monotonicity = monotonicity.Value }
			: (Ulid.GenerationOptions?)null;

		lock (_staticStateLock)
		{
			var originalOptions = Ulid.DefaultGenerationOptions;
			try
			{
				Ulid.DefaultGenerationOptions = originalOptions with { Monotonicity = defaultMonotonicity };

				// The Ulid generation with the same timestamp depends on a static last-ulid state.
				Ulid.New(options); // Reset the static state by generating a ULID with a fresh, higher timestamp.

				// Act
				var ulid1 = Ulid.New(timestamp, options);
				var ulid2 = Ulid.New(timestamp, options);

				// Assert
				Assert.NotEqual(ulid1, ulid2);

				Assert.True(ulid1.Time.ToUnixTimeMilliseconds() <= ulid2.Time.ToUnixTimeMilliseconds());
				Assert.True(timestamp <= ulid1.Time.ToUnixTimeMilliseconds());
				Assert.True(timestamp <= ulid2.Time.ToUnixTimeMilliseconds());

				var expectedMonotonicity = monotonicity ?? defaultMonotonicity;
				if (expectedMonotonicity != Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic)
				{
					Assert.True(ulid1.AsByteSpan().SequenceCompareTo(ulid2.AsByteSpan()) < 0);
					Assert.True(ulid1 < ulid2);
				}
			}
			finally
			{
				Ulid.DefaultGenerationOptions = originalOptions;
			}
		}
	}

	[Theory]
	[CombinatorialData]
	public void New_WithAllGenerationOptions_ShouldGenerateCorrectly(
		Ulid.GenerationOptions.MonotonicityOptions monotonicity,
		Ulid.GenerationOptions.RandomSourceOptions initialRandomSource,
		Ulid.GenerationOptions.RandomSourceOptions incrementRandomSource
	)
	{
		// Arrange
		var options = new Ulid.GenerationOptions
		{
			Monotonicity = monotonicity,
			InitialRandomSource = initialRandomSource,
			IncrementRandomSource = incrementRandomSource
		};
		var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		lock (_staticStateLock)
		{
			Ulid.New(options); // Ensure _lastUlid is recent

			// Act
			var ulid1 = Ulid.New(timestamp, options);
			var ulid2 = Ulid.New(timestamp, options);

			// Assert
			Assert.NotEqual(ulid1, ulid2);

			if (monotonicity != Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic)
			{
				Assert.True(ulid1.Time.ToUnixTimeMilliseconds() <= ulid2.Time.ToUnixTimeMilliseconds());
				Assert.True(ulid1 < ulid2, "ULIDs should be monotonic");
			}
		}
	}
}