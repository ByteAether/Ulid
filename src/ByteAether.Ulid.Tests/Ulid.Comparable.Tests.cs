namespace ByteAether.Ulid.Tests;

public class UlidComparableTests
{
	[Fact]
	public void CompareTo_SameUlid_ShouldReturnZero()
	{
		// Arrange
		var ulid1 = Ulid.New();
		var ulid2 = ulid1;

		// Act
		var comparisonResult = ulid1.CompareTo(ulid2);

		// Assert
		Assert.Equal(0, comparisonResult);
		Assert.False(ulid1 < ulid2);
		Assert.True(ulid1 <= ulid2);
		Assert.True(ulid1 >= ulid2);
		Assert.False(ulid1 > ulid2);
	}

	[Fact]
	public void CompareTo_CompareToNewerUlid_ShouldReturnNegative()
	{
		// Arrange
		var ulid1 = Ulid.New(true);
		var ulid2 = Ulid.New(true);

		// Act
		var comparisonResult = ulid1.CompareTo(ulid2);

		// Assert
		Assert.True(comparisonResult < 0, $"{ulid1} < {ulid2}: {comparisonResult}");
		Assert.True(ulid1 < ulid2);
		Assert.True(ulid1 <= ulid2);
		Assert.False(ulid1 > ulid2);
		Assert.False(ulid1 >= ulid2);
	}

	[Fact]
	public void CompareTo_CompareToOlderUlid_ShouldReturnPositive()
	{
		// Arrange
		var ulid1 = Ulid.New(true);
		var ulid2 = Ulid.New(true);

		// Act
		var comparisonResult = ulid2.CompareTo(ulid1);

		// Assert
		Assert.True(comparisonResult > 0, $"{ulid2} > {ulid1}: {comparisonResult}");
		Assert.True(ulid2 > ulid1);
		Assert.True(ulid2 >= ulid1);
		Assert.False(ulid2 < ulid1);
		Assert.False(ulid2 <= ulid1);
	}

	[Fact]
	public void CompareTo_NullUlid_ShouldReturnPositive()
	{
		// Arrange
		var ulid = Ulid.New();

		// Act
		var comparisonResult = ulid.CompareTo(null);

		// Assert
		Assert.True(comparisonResult > 0);
	}

	[Fact]
	public void CompareTo_DifferentType_ShouldThrowArgumentException()
	{
		// Arrange
		var ulid = Ulid.New();

		// Act & Assert
		Assert.Throws<ArgumentException>(() => ulid.CompareTo(new object()));
	}
}
