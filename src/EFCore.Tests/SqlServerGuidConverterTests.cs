using System.Data.SqlTypes; // For SqlGuid testing

namespace ByteAether.Ulid.EntityFrameworkCore.Tests;

public class SqlServerGuidConverterTests
{
	[Fact]
	public void Converter_ShouldBePerfectRoundTrip()
	{
		// Arrange
		var originalUlid = Ulid.New();

		// Act
		var sqlGuid = UlidToSqlServerGuidConverter.ToSqlServerGuid(originalUlid);
		var roundTrippedUlid = UlidToSqlServerGuidConverter.FromSqlServerGuid(sqlGuid);

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
		var firstGuid = UlidToSqlServerGuidConverter.ToSqlServerGuid(firstUlid);
		var secondGuid = UlidToSqlServerGuidConverter.ToSqlServerGuid(secondUlid);

		// Wrap them in .NET's SqlGuid, which uses the exact same sorting rules as SQL Server engine
		var sqlGuid1 = new SqlGuid(firstGuid);
		var sqlGuid2 = new SqlGuid(secondGuid);

		// Assert - The second one must be logically greater than the first
		Assert.True((sqlGuid2 > sqlGuid1).IsTrue, $"Sorting failed. {firstUlid} and {secondUlid} did not preserve order in SqlGuid.");
	}

	[Fact]
	public void StandardGuid_ShouldAlwaysFail_ChronologicalSortingInSqlServer()
	{
		// Arrange - Create a fixed 10-byte random array
		Span<byte> sharedRandom = stackalloc byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		// Generate two ULIDs with a chronological difference but IDENTICAL randomness
		var firstUlid = Ulid.New(DateTimeOffset.UtcNow.AddMinutes(-5), sharedRandom);
		var secondUlid = Ulid.New(DateTimeOffset.UtcNow, sharedRandom);

		// Act
		var firstStandardGuid = firstUlid.ToGuid();
		var secondStandardGuid = secondUlid.ToGuid();

		var sqlGuid1 = new SqlGuid(firstStandardGuid);
		var sqlGuid2 = new SqlGuid(secondStandardGuid);

		// Assert - Since the random components are identical, SqlGuid falls back to lower priority bytes.
		// Due to Little-Endian formatting of Guid's Data1/Data2 structures,
		// the higher timestamp actually evaluates as LOGICALLY SMALLER in SqlGuid's native byte comparison.
		Assert.False((sqlGuid2 > sqlGuid1).IsTrue, "Standard Guid accidentally sorted chronologically.");
	}
}