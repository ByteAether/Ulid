namespace ByteAether.Ulid.Tests;

public class UlidNewTests
{
	// A lock object to synchronize tests that rely on shared static state in the Ulid class,
	// preventing race conditions and ensuring test isolation.

	// ReSharper disable once ChangeFieldTypeToSystemThreadingLock for older .net compatibility
	private static readonly object _staticStateLock = new();

	// We need safe DateTimeOffset values for monotonicity
	private static readonly DateTimeOffset _lastTimestamp = DateTimeOffset.UtcNow.AddMinutes(1);
	private static int _timestampOffsetCounter;

	private static DateTimeOffset GetDateTimeOffset() => _lastTimestamp.AddSeconds(++_timestampOffsetCounter);

	/// <summary>
	/// A controllable random provider for testing purposes. It returns pre-configured byte sequences.
	/// </summary>
	private class ControllableRandomProvider(params byte[][] ByteSequences) : IRandomProvider
	{
		private readonly Queue<byte[]> _byteSequences = new(ByteSequences);

		public void GetBytes(Span<byte> buffer)
		{
			var bytes = _byteSequences.Dequeue();
			bytes.CopyTo(buffer);
		}
	}

	[Fact]
	public void ToByteArray_FromSpecificData_ShouldConvertToByteArrayAndBack()
	{
		// Arrange
		var timestamp = 1617277200000L; // 2021-04-01 12:00:00 UTC
		var randomBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
		var ulid = Ulid.New(timestamp, randomBytes);

		// Act
		var byteArray = ulid.ToByteArray();
		var ulidFromBytes = Ulid.New(byteArray);

		// Assert
		Assert.Equal(16, byteArray.Length);
		Assert.Equal(ulid, ulidFromBytes);
	}

	[Fact]
	public void New_WithTimestampAndRandom_ShouldGenerateSameUlid()
	{
		// Arrange
		var timestamp = DateTimeOffset.UtcNow;
		var random = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		// Act
		var ulid1 = Ulid.New(timestamp, random);
		var ulid2 = Ulid.New(timestamp, random);

		// Assert
		Assert.Equal(ulid1, ulid2);
	}

	[Fact]
	public void New_WithTimestampAndRandom_ShouldGenerateCorrectTimestampAndRandom()
	{
		// Arrange
		var timestamp = DateTimeOffset.UtcNow;
		var random = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		// Act
		var ulid = Ulid.New(timestamp, random);

		// Assert
		Assert.Equal(timestamp.ToUnixTimeMilliseconds(), ulid.Time.ToUnixTimeMilliseconds());
		Assert.Equal(random, ulid.Random.ToArray());
	}

	[Fact]
	public void New_NonMonotonic_CanProduceSmallerUlids()
	{
		lock (_staticStateLock)
		{
			// Arrange
			var timestamp = GetDateTimeOffset().ToUnixTimeMilliseconds();
			var random1 = new byte[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
			var random2 = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

			var initialRandomProvider = new ControllableRandomProvider(random1, random2);
			var options = new Ulid.GenerationOptions
			{
				Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic,
				InitialRandomSource = initialRandomProvider
			};

			// Act
			var ulid1 = Ulid.New(timestamp, options);
			var ulid2 = Ulid.New(timestamp, options);

			// Assert
			Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
			Assert.Equal(random1, ulid1.Random.ToArray());

			Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());
			Assert.Equal(random2, ulid2.Random.ToArray());
		}
	}

	[Fact]
	public void New_MonotonicIncrement_ShouldOverflowToTimestamp()
	{
		lock (_staticStateLock)
		{
			// Arrange
			var timestamp = GetDateTimeOffset().ToUnixTimeMilliseconds();
			var initialRandom = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD };

			var initialRandomProvider = new ControllableRandomProvider(initialRandom);

			var options = new Ulid.GenerationOptions
			{
				Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicIncrement,
				InitialRandomSource = initialRandomProvider
			};

			// Act
			var ulid1 = Ulid.New(timestamp, options); // Random ...FD
			var ulid2 = Ulid.New(timestamp, options); // Random ...FE (incremented)
			var ulid3 = Ulid.New(timestamp, options); // Random ...FF (incremented)
			var ulid4 = Ulid.New(timestamp, options); // Overflow

			// Assert
			Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
			Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());
			Assert.Equal(timestamp, ulid3.Time.ToUnixTimeMilliseconds());
			Assert.Equal(timestamp + 1, ulid4.Time.ToUnixTimeMilliseconds());

			Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD }, ulid1.Random.ToArray());
			Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }, ulid2.Random.ToArray());
			Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, ulid3.Random.ToArray());
			Assert.Equal(new byte[10], ulid4.Random.ToArray());
		}
	}

	[Theory]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte, 1)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom2Byte, 2)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom3Byte, 3)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte, 4)]
	public void New_MonotonicRandom_ShouldIncrementCorrectly(
		Ulid.GenerationOptions.MonotonicityOptions monotonicity,
		int incrementSize
	)
	{
		lock (_staticStateLock)
		{
			// Arrange
			var timestamp = GetDateTimeOffset().ToUnixTimeMilliseconds();

			var initialRandom = new byte[10];
			initialRandom[9] = 0xA; // 10

			var increment = new byte[incrementSize];
			increment[incrementSize - 1] = 4; // 10 + 4

			var incrementedRandom = new byte[10];
			incrementedRandom[9] = 15; // 10 + 4 + 1 : +1 comes from base implementation of Ulid

			var initialRandomProvider = new ControllableRandomProvider(initialRandom);
			var incrementRandomProvider = new ControllableRandomProvider(increment);

			var options = new Ulid.GenerationOptions
			{
				Monotonicity = monotonicity,
				InitialRandomSource = initialRandomProvider,
				IncrementRandomSource = incrementRandomProvider
			};

			// Act
			var ulid1 = Ulid.New(timestamp, options);
			var ulid2 = Ulid.New(timestamp, options);

			// Assert
			Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
			Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());

			Assert.Equal(initialRandom, ulid1.Random.ToArray());
			Assert.Equal(incrementedRandom, ulid2.Random.ToArray());
		}
	}

	[Theory]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte, 1)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom2Byte, 2)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom3Byte, 3)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte, 4)]
	public void New_MonotonicRandom_ShouldCarryOverIncrement(
		Ulid.GenerationOptions.MonotonicityOptions monotonicity, int incrementSize
	)
	{
		lock (_staticStateLock)
		{
			// Arrange
			var timestamp = GetDateTimeOffset().ToUnixTimeMilliseconds();

			var initialRandom = new byte[10];
			initialRandom[9] = 0xFE; // Max - 2

			var increment = new byte[incrementSize];
			increment[incrementSize - 1] = 0x01; // 1 as the other +1 comes from base implementation

			var initialRandomProvider = new ControllableRandomProvider(initialRandom);
			var incrementRandomProvider = new ControllableRandomProvider(increment);

			var options = new Ulid.GenerationOptions
			{
				Monotonicity = monotonicity,
				InitialRandomSource = initialRandomProvider,
				IncrementRandomSource = incrementRandomProvider
			};

			// Act
			var ulid1 = Ulid.New(timestamp, options);
			var ulid2 = Ulid.New(timestamp, options);

			// Assert
			Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
			Assert.Equal(timestamp, ulid2.Time.ToUnixTimeMilliseconds());
			Assert.Equal(initialRandom, ulid1.Random.ToArray());

			var expectedRandom = new byte[10];
			expectedRandom[8] = 0x01; // ...0100
			expectedRandom[9] = 0x00;

			Assert.Equal(expectedRandom, ulid2.Random.ToArray());
		}
	}


	[Theory]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte, 1)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom2Byte, 2)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom3Byte, 3)]
	[InlineData(Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte, 4)]
	public void New_MonotonicRandom_ShouldOverflowToTimestamp(
		Ulid.GenerationOptions.MonotonicityOptions monotonicity, int incrementSize
	)
	{
		lock (_staticStateLock)
		{
			// Arrange
			var timestamp = GetDateTimeOffset().ToUnixTimeMilliseconds();
			var initialRandom = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
			//Set all the last bytes to 0x00 that should be incremented later
			for(var i = initialRandom.Length - incrementSize; i < initialRandom.Length; i++)
			{
				initialRandom[i] = 0x00;
			}

			var increment = Enumerable.Repeat<byte>(0xFF, incrementSize).ToArray();
			// overflow +1 comes from the base implementation of Ulid

			var initialRandomProvider = new ControllableRandomProvider(initialRandom);
			var incrementRandomProvider = new ControllableRandomProvider(increment);

			var options = new Ulid.GenerationOptions
			{
				Monotonicity = monotonicity,
				InitialRandomSource = initialRandomProvider,
				IncrementRandomSource = incrementRandomProvider
			};

			// Act
			var ulid1 = Ulid.New(timestamp, options); // Random is all 0xFF
			var ulid2 = Ulid.New(timestamp, options); // This should overflow

			// Assert
			Assert.Equal(timestamp, ulid1.Time.ToUnixTimeMilliseconds());
			Assert.Equal(initialRandom, ulid1.Random.ToArray());

			Assert.Equal(timestamp + 1, ulid2.Time.ToUnixTimeMilliseconds());
			Assert.Equal(new byte[10], ulid2.Random.ToArray());
		}
	}
}