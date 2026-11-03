namespace ByteAether.Ulid.Tests;

public class UlidGuidTests
{
	[Fact]
	public void FromAndToGuid_ShouldPreserveValue()
	{
		// Arrange
		var ulid = Ulid.New(); // Generate a new ULID

		// Act
		var guid = ulid.ToGuid(); // Convert ULID to GUID
		var ulidFromGuid = Ulid.New(guid); // Create a new ULID from the GUID

		// Assert
		Assert.NotEqual(Guid.Empty, guid); // Ensure the GUID is not empty
		Assert.Equal(ulid, ulidFromGuid); // Ensure the ULID matches after conversion
	}

	[Fact]
	public void ToGuid_ShouldKeepTheByteOrderOfTheStringRepresentation()
	{
		// Arrange
		// The canonical text of a GUID lists its bytes in the same order as the bytes of a ULID,
		// whereas the in-memory layout of a Guid stores its first three fields in little-endian order.
		byte[] ulidBytes = [0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff];
		var expected = new Guid("00112233-4455-6677-8899-aabbccddeeff");
		var ulid = Ulid.New(ulidBytes);

		// Act & Assert
		Assert.Equal(expected, ulid.ToGuid());
		Assert.Equal(expected, (Guid)ulid);
		Assert.Equal(ulid, Ulid.New(expected));
		Assert.Equal(ulidBytes, Ulid.New(expected).ToByteArray());
	}

	[Fact]
	public void FromAndToGuid_RandomValues_ShouldMatchTheGuidByteOrder()
	{
		// Arrange
		var random = new Random(4711);
		var bytes = new byte[16];

		for (var i = 0; i < 10_000; i++)
		{
			random.NextBytes(bytes);

			// Act
			var ulid = Ulid.New(bytes);
			var guid = ulid.ToGuid();

			// Assert
			Assert.Equal(ulid.ToString(), Ulid.New(guid).ToString());
			Assert.Equal(guid.ToString("N"), Convert.ToHexString(bytes).ToLowerInvariant());
			Assert.Equal(ulid, Ulid.New(guid));
		}
	}

	[Fact]
	public void FromAndToGuid_ImplicitConversion_ShouldPreserveValue()
	{
		// Arrange
		var ulid = Ulid.New(); // Generate a new ULID

		// Act
		Guid guid = ulid; // Implicitly convert ULID to GUID
		Ulid ulidFromGuid = guid; // Implicitly convert GUID back to ULID

		// Assert
		Assert.NotEqual(Guid.Empty, guid); // Ensure the GUID is not empty
		Assert.Equal(ulid, ulidFromGuid); // Ensure the ULID matches after conversion
	}
}
