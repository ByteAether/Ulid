using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Data.Sqlite;
using Xunit;

namespace ByteAether.Ulid.LinqToDB.IntegrationTests;

public class TestEntity
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column, NotNull]
    public Ulid SystemUlid { get; set; }

    [Column, Nullable]
    public Ulid? NullableUlid { get; set; }
}

public class RelatedChildEntity
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column, NotNull]
    public Ulid ParentSystemUlid { get; set; }

    [Column, Nullable]
    public string Description { get; set; } = string.Empty;
}

public class UlidLinqToDbIntegrationTests : IDisposable
{
	private readonly SqliteConnection _connection;

    public UlidLinqToDbIntegrationTests()
    {
	    // Shared open connection to preserve database scope for individual test runs
        _connection = new("Filename=:memory:");
        _connection.Open();
    }

    private DataConnection CreateConnection(UlidStorageFormat format)
    {
        var options = new DataOptions()
	        .UseSQLite()
	        .UseConnection(_connection)
	        .RegisterUlid(format);

        var connection = new DataConnection(options);

        // Natively construct volatile mock schema tables for testing execution paths
        try { connection.CreateTable<TestEntity>(); } catch { /* Ignore table already exists if hit */ }
        try { connection.CreateTable<RelatedChildEntity>(); } catch { /* Ignore table already exists if hit */ }

        return connection;
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task LinqToDB_ShouldSuccessfullyRoundTrip_BothNonNullAndNullableUlids(UlidStorageFormat format)
    {
        // Arrange
        var originalUlid = Ulid.New();
        var entity = new TestEntity
        {
            SystemUlid = originalUlid,
            NullableUlid = null
        };

        // Act - Step 1: Write to database
        await using (var writeContext = CreateConnection(format))
        {
            await writeContext.InsertAsync(entity);
        }

        // Act - Step 2: Read back via an isolated, stateless connection
        await using (var readContext = CreateConnection(format))
        {
            var dbEntity = await readContext.GetTable<TestEntity>()
                .FirstOrDefaultAsync(e => e.SystemUlid == originalUlid);

            // Assert
            Assert.NotNull(dbEntity);
            Assert.Equal(originalUlid, dbEntity.SystemUlid);
            Assert.Null(dbEntity.NullableUlid);

            // Step 3: Test update conversion behavior on nullable property types
            var updatedUlid = Ulid.New();
            dbEntity.NullableUlid = updatedUlid;
            await readContext.UpdateAsync(dbEntity);
        }

        // Act - Step 4: Validate update persistence
        await using (var verifyContext = CreateConnection(format))
        {
            var dbEntity = await verifyContext.GetTable<TestEntity>().FirstOrDefaultAsync();
            Assert.NotNull(dbEntity);
            Assert.Equal(originalUlid, dbEntity.SystemUlid);
            Assert.NotNull(dbEntity.NullableUlid);

            // Step 5: Test reversing a value back to null (Null-to-Null round trip)
            dbEntity.NullableUlid = null;
            await verifyContext.UpdateAsync(dbEntity);
        }

        // Act - Step 6: Final check that reverting to null persisted properly
        await using (var finalVerifyContext = CreateConnection(format))
        {
	        var dbEntity = await finalVerifyContext.GetTable<TestEntity>().FirstOrDefaultAsync();
	        Assert.NotNull(dbEntity);
	        Assert.Null(dbEntity.NullableUlid);
        }
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    //[InlineData(UlidStorageFormat.Guid)] // Not correct on SQLite
    //[InlineData(UlidStorageFormat.SqlServerGuid)] // Not supported on SQLite - should work on MSSQL
    public async Task LinqToDB_ShouldTranslateLINQRangeQueries_ProperlyWithParameters(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateConnection(format);

        var minUlid = Ulid.MinAt(DateTimeOffset.UtcNow.AddDays(-1));
        var targetUlid = Ulid.New();
        var maxUlid = Ulid.MaxAt(DateTimeOffset.UtcNow.AddDays(1));

        await context.InsertAsync(new TestEntity { SystemUlid = targetUlid });

        // Act - Validate that LinqToDB command tree translates logical bounds matching type conversions
        var results = await context.GetTable<TestEntity>()
            .Where(e => e.SystemUlid >= minUlid && e.SystemUlid <= maxUlid)
            .ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal(targetUlid, results[0].SystemUlid);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task LinqToDB_ShouldSuccessfullyProject_UlidToAnonymousAndDtoTypes(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateConnection(format);
        var targetUlid = Ulid.New();

        await context.InsertAsync(new TestEntity { SystemUlid = targetUlid, NullableUlid = Ulid.New() });

        // Act - Step 1: Query compilation evaluation over anonymous shape allocations
        var anonymousResult = await context.GetTable<TestEntity>()
            .Select(e => new { e.Id, e.SystemUlid, e.NullableUlid })
            .FirstOrDefaultAsync(e => e.SystemUlid == targetUlid);

        // Act - Step 2: Extract scalar data projections
        var rawUlidList = await context.GetTable<TestEntity>()
            .Select(e => e.SystemUlid)
            .ToListAsync();

        // Assert
        Assert.NotNull(anonymousResult);
        Assert.Equal(targetUlid, anonymousResult.SystemUlid);
        Assert.NotNull(anonymousResult.NullableUlid);

        Assert.Contains(targetUlid, rawUlidList);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task LinqToDB_ShouldTranslateContainsQuery_WhenUsingUlidCollections(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateConnection(format);

        var targetUlid = Ulid.New();

        await context.InsertAsync(new TestEntity { NullableUlid = targetUlid });
        await context.InsertAsync(new TestEntity { NullableUlid = null });

        // Strategy: Include null directly inside the searchable target criteria collection
        var searchCriteria = new Ulid?[] { targetUlid, null };

        // Act
        var results = await context.GetTable<TestEntity>()
            .Where(e => searchCriteria.Contains(e.NullableUlid))
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, e => e.NullableUlid == targetUlid);
        Assert.Contains(results, e => e.NullableUlid == null);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    //[InlineData(UlidStorageFormat.Guid)] // Not correct on SQLite
    //[InlineData(UlidStorageFormat.SqlServerGuid)] // Not correct on SQLite - should work on MSSQL
    public async Task LinqToDB_ShouldMaintainChronologicalOrder_WhenOrderingByUlid(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateConnection(format);

        var first = Ulid.New();
        await Task.Delay(10); // Enforce clear hardware timestamp increments
        var second = Ulid.New();
        await Task.Delay(10);
        var third = Ulid.New();

        // Insert scrambled chronological payloads
        await context.InsertAsync(new TestEntity { SystemUlid = second });
        await context.InsertAsync(new TestEntity { SystemUlid = third });
        await context.InsertAsync(new TestEntity { SystemUlid = first });

        // Act
        var orderedList = await context.GetTable<TestEntity>()
            .OrderBy(e => e.SystemUlid)
            .ToListAsync();

        // Assert
        Assert.Equal(3, orderedList.Count);
        Assert.Equal(first, orderedList[0].SystemUlid);
        Assert.Equal(second, orderedList[1].SystemUlid);
        Assert.Equal(third, orderedList[2].SystemUlid);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task LinqToDB_ShouldSuccessfullyExecuteJoins_OnUlidProperties(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateConnection(format);

        var parentUlid = Ulid.New();
        await context.InsertAsync(new TestEntity { SystemUlid = parentUlid });
        await context.InsertAsync(new RelatedChildEntity { ParentSystemUlid = parentUlid, Description = "Child linked via ULID" });

        // Act - Enforce expression evaluation trees across relational predicates
        var joinResult = await context.GetTable<TestEntity>()
            .Join(
                context.GetTable<RelatedChildEntity>(),
                parent => parent.SystemUlid,
                child => child.ParentSystemUlid,
                (parent, child) => new { parent.Id, parent.SystemUlid, child.Description }
            )
            .FirstOrDefaultAsync(x => x.SystemUlid == parentUlid);

        // Assert
        Assert.NotNull(joinResult);
        Assert.Equal(parentUlid, joinResult.SystemUlid);
        Assert.Equal("Child linked via ULID", joinResult.Description);
    }

    [Theory]
    [InlineData(UlidStorageFormat.String, DataType.Char)]
    [InlineData(UlidStorageFormat.Binary, DataType.Binary)]
    [InlineData(UlidStorageFormat.Guid, DataType.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid, DataType.Guid)]
    public async Task SchemaMetadata_ShouldRegisterCorrectDataTypeHints(UlidStorageFormat format, DataType expectedDataType)
    {
        // Arrange
        await using var context = CreateConnection(format);
        var schema = context.MappingSchema;

        // Act
        var columnInformation = schema.GetDataType(typeof(Ulid));

        // Assert - Ensures that LinqToDB maps to proper native column variants instead of fallback configurations
        Assert.Equal(expectedDataType, columnInformation.Type.DataType);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}