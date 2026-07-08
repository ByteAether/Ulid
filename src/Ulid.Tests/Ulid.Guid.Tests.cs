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
