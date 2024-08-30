namespace ByteAether.Ulid.Tests;

public class UlidEquatableTests
{
	[Fact]
	public void Equals_SameUlid_ReturnsTrue()
	{
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		Assert.True(ulid1.Equals(ulid2));
	}

	[Fact]
	public void Equals_DifferentUlid_ReturnsFalse()
	{
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		Assert.False(ulid1.Equals(ulid2));
	}

	[Fact]
	public void Equals_Null_ReturnsFalse()
	{
		var ulid = Ulid.New();

		Assert.False(ulid.Equals(null));
	}

	[Fact]
	public void Equals_WrongObject_ReturnsFalse()
	{
		var ulid = Ulid.New();

		Assert.False(ulid.Equals(new object()));
	}
}
