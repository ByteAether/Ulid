using System.Data.SqlTypes;

namespace ByteAether.Ulid.DB.Shared.Tests;

public class MsSqlUlidShufflerTests
{
	[Fact]
	public void Converter_ShouldBePerfectRoundTrip()
	{
		// Arrange
		var originalUlid = Ulid.New();

		// Act
		var sqlGuid = MsSqlUlidShuffler.ToSqlServerGuid(originalUlid);
		var roundTrippedUlid = MsSqlUlidShuffler.FromSqlServerGuid(sqlGuid);

		// Assert
		Assert.Equal(originalUlid, roundTrippedUlid);
	}

	[Fact]
	public void ToSqlServerGuid_ShouldSortChronologicallyInSqlServer()
	{
		// Arrange
		var firstUlid = Ulid.New(DateTimeOffset.UtcNow.AddMinutes(-5));
		var secondUlid = Ulid.New(DateTimeOffset.UtcNow);

		// Act - Convert using our custom shuffler
		var firstGuid = MsSqlUlidShuffler.ToSqlServerGuid(firstUlid);
		var secondGuid = MsSqlUlidShuffler.ToSqlServerGuid(secondUlid);

		// Wrap them in .NET's SqlGuid, which uses the exact same sorting rules as SQL Server engine
		var sqlGuid1 = new SqlGuid(firstGuid);
		var sqlGuid2 = new SqlGuid(secondGuid);

		// Assert - The second one must be logically greater than the first
		Assert.True((sqlGuid2 > sqlGuid1).Value, $"Sorting failed. {firstUlid} and {secondUlid} did not preserve order in SqlGuid.");
	}
}