namespace ByteAether.Ulid.Tests;

public class UlidComparableTests
{
	[Fact]
	public void CompareTo_SameUlid_ReturnsZero()
	{
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		Assert.Equal(0, ulid1.CompareTo(ulid2));
		Assert.True(ulid1 == ulid2);
	}

	[Fact]
	public void CompareTo_CompareToNewer_CorrectReturn()
	{
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		var result = ulid1.CompareTo(ulid2);

		Assert.True(result < 0, $"{ulid1} < {ulid2}: {result}");
		Assert.True(ulid1 < ulid2);
	}

	[Fact]
	public void CompareTo_CompareToOlder_CorrectReturn()
	{
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		var result = ulid2.CompareTo(ulid1);

		Assert.True(result > 0, $"{ulid2} > {ulid1}: {result}");
		Assert.True(ulid2 > ulid1);
	}

	[Fact]
	public void CompareTo_NullUlid_ReturnsOne()
	{
		var ulid = Ulid.New();

		Assert.True(ulid.CompareTo(null) > 0);
	}

	[Fact]
	public void CompareTo_DifferentType_ThrowsArgumentException()
	{
		var ulid = Ulid.New();

		Assert.Throws<ArgumentException>(() => ulid.CompareTo(new object()));
	}
}