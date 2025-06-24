using System.Runtime.CompilerServices;

namespace ByteAether.Ulid.Tests;
public class UlidTests
{
	[Fact]
	public void EmptyUlid_ShouldBeDefault()
	{
		// Arrange
		var ulid = Ulid.Empty;

		// Assert
		Assert.Equal(default, ulid);
		Assert.Equal(new byte[16], ulid.ToByteArray());
	}

	private static readonly byte[] _sampleUlidBytes =
		[
            // Timestamp (6 bytes)
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
            // Random (10 bytes)
            0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
		];

	[Fact]
	public void TimeBytes_ShouldReturnFirst6Bytes()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);

		// Act
		var timeBytes = ulid.TimeBytes;

		// Assert
		Assert.Equal(_sampleUlidBytes.Take(6).ToArray(), timeBytes.ToArray());
	}

	[Fact]
	public void Random_ShouldReturnLast10Bytes()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);

		// Act
		var randomBytes = ulid.Random;

		// Assert
		Assert.Equal(_sampleUlidBytes.Skip(6).Take(10).ToArray(), randomBytes.ToArray());
	}

	[Fact]
	public void Time_ShouldReturnCorrectDateTimeOffset()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);
		var expectedTimestampMilliseconds =
			((long)_sampleUlidBytes[0] << 40) |
			((long)_sampleUlidBytes[1] << 32) |
			((long)_sampleUlidBytes[2] << 24) |
			((long)_sampleUlidBytes[3] << 16) |
			((long)_sampleUlidBytes[4] << 8) |
			_sampleUlidBytes[5];
		var expectedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(expectedTimestampMilliseconds);

		// Act
		var timestamp = ulid.Time;

		// Assert
		Assert.Equal(expectedTimestamp, timestamp);
	}

	[Fact]
	public void AsByteSpan_ShouldReturnAll16Bytes()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);

		// Act
		var byteSpan = ulid.AsByteSpan();

		// Assert
		Assert.Equal(_sampleUlidBytes, byteSpan.ToArray());
	}

	[Fact]
	public void ToByteArray_ShouldReturnAll16Bytes()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);

		// Act
		var byteArray = ulid.ToByteArray();

		// Assert
		Assert.Equal(_sampleUlidBytes, byteArray);
	}

	[Fact]
	public void ToByteArray_ShouldNotMutateOriginalBytes()
	{
		// Arrange
		var ulid = CreateUlid(_sampleUlidBytes);
		var byteArray = ulid.ToByteArray();

		// Act
		byteArray[0] = 0xFF;

		// Assert
		Assert.NotEqual(byteArray, ulid.AsByteSpan().ToArray());
	}

	// Helper to create a ULID instance from a byte array.
	private static Ulid CreateUlid(byte[] bytes)
	{
		if (bytes.Length != 16)
		{
			throw new ArgumentException("ULID byte array must be exactly 16 bytes.");
		}

		var ulid = new Ulid();
		unsafe
		{
			fixed (byte* ptr = bytes)
			{
				ulid = Unsafe.Read<Ulid>(ptr);
			}
		}

		return ulid;
	}
}
