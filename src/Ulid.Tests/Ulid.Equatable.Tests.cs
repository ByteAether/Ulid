namespace ByteAether.Ulid.Tests;

public class UlidEquatableTests
{
	[Fact]
	public void GetHashCode_SameUlid_ShouldReturnEqualHashCodes()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		// Act
		var hashCode1 = ulid1.GetHashCode();
		var hashCode2 = ulid2.GetHashCode();

		// Assert
		Assert.Equal(hashCode1, hashCode2);
	}

	[Fact]
	public void GetHashCode_DifferentUlids_ShouldReturnDifferentHashCodes()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		// Act
		var hashCode1 = ulid1.GetHashCode();
		var hashCode2 = ulid2.GetHashCode();

		// Assert
		Assert.NotEqual(hashCode1, hashCode2);
	}

	[Fact]
	public void Equals_SameUlid_ShouldReturnTrue()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		// Act & Assert
		Assert.True(ulid1.Equals(ulid2));
		Assert.True(ulid1 == ulid2);
		Assert.False(ulid1 != ulid2);
	}

	[Fact]
	public void Equals_DifferentUlids_ShouldReturnFalse()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		// Act & Assert
		Assert.False(ulid1.Equals(ulid2));
		Assert.False(ulid1 == ulid2);
		Assert.True(ulid1 != ulid2);
	}

	[Fact]
	public void Equals_Null_ShouldReturnFalse()
	{
		// Arrange
		var ulid = Ulid.New();

		// Act & Assert
		Assert.False(ulid.Equals((object?)null));
	}

	[Fact]
	public void Equals_WrongObject_ShouldReturnFalse()
	{
		// Arrange
		var ulid = Ulid.New();
		var wrongObject = new object();

		// Act & Assert
		Assert.False(ulid.Equals(wrongObject));
	}

	[Fact]
	public void Equals_SameAsObject_ShouldReturnTrue()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		// Act & Assert
		Assert.True(ulid1.Equals((object)ulid2));
	}
}