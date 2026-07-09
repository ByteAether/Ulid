using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ByteAether.Ulid.EntityFrameworkCore.IntegrationTests;

public class UlidEntityFrameworkIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public UlidEntityFrameworkIntegrationTests()
    {
        // SQLite in-memory databases vanish when the connection closes.
        // We open it explicitly here to keep the schema alive for the duration of each test.
        _connection = new("Filename=:memory:");
        _connection.Open();
    }

    private TestDbContext CreateContext(UlidStorageFormat format)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new TestDbContext(options, format);

        // Ensure the schema reflects the exact applied converters
        context.Database.EnsureCreated();

        // Completely purge the internal cache so entries must be read directly from the database schema
        context.ChangeTracker.Clear();

        return context;
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task EFCore_ShouldSuccessfullyRoundTrip_BothNonNullAndNullableUlids(UlidStorageFormat format)
    {
        // Arrange
	    var originalUlid = Ulid.New();
	    var entity = new TestEntity
	    {
	        SystemUlid = originalUlid,
	        NullableUlid = null
	    };

	    // Act - Step 1: Write to the database
	    await using (var writeContext = CreateContext(format))
	    {
	        writeContext.TestEntities.Add(entity);
	        await writeContext.SaveChangesAsync();
	    }

	    // Act - Step 2: Read back from an isolated context instance & Query via Null comparison
	    await using (var readContext = CreateContext(format))
	    {
	        var dbEntity = await readContext.TestEntities.FirstOrDefaultAsync(e => e.SystemUlid == originalUlid);

	        // Verify querying directly by matching a null column works flawlessly
	        var nullQueryEntity = await readContext.TestEntities.FirstOrDefaultAsync(e => e.NullableUlid == null);

	        // Assert
	        Assert.NotNull(dbEntity);
	        Assert.NotNull(nullQueryEntity);
	        Assert.Equal(originalUlid, dbEntity.SystemUlid);
	        Assert.Null(dbEntity.NullableUlid);

	        // Step 3: Test update behavior on Nullable property
	        var updatedUlid = Ulid.New();
	        dbEntity.NullableUlid = updatedUlid;
	        readContext.TestEntities.Update(dbEntity);
	        await readContext.SaveChangesAsync();
	    }

	    // Act - Step 4: Validate update retrieval
	    await using (var verifyContext = CreateContext(format))
	    {
	        var dbEntity = await verifyContext.TestEntities.FirstOrDefaultAsync();
	        Assert.NotNull(dbEntity);
	        Assert.Equal(originalUlid, dbEntity.SystemUlid);
	        Assert.NotNull(dbEntity.NullableUlid);

	        // Step 5: Test reversing a value back to null (Null-to-Null round trip)
	        dbEntity.NullableUlid = null;
	        verifyContext.TestEntities.Update(dbEntity);
	        await verifyContext.SaveChangesAsync();
	    }

	    // Act - Step 6: Final check that reverting to null persisted properly
	    await using (var finalVerifyContext = CreateContext(format))
	    {
	        var dbEntity = await finalVerifyContext.TestEntities.FirstOrDefaultAsync();
	        Assert.NotNull(dbEntity);
	        Assert.Null(dbEntity.NullableUlid);
	    }
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    public async Task EFCore_ShouldTranslateLINQRangeQueries_ProperlyWithParameters(UlidStorageFormat format)
    {
        // Arrange
        await using var context = CreateContext(format);

        var minUlid = Ulid.MinAt(DateTimeOffset.UtcNow.AddDays(-1));
        var targetUlid = Ulid.New(); // Current timestamp
        var maxUlid = Ulid.MaxAt(DateTimeOffset.UtcNow.AddDays(1));

        context.TestEntities.Add(new(){ SystemUlid = targetUlid });
        await context.SaveChangesAsync();

        // Act - Evaluate if EF translation properly maps parameter types to database lookups
        var results = await context.TestEntities
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
    public async Task EFCore_ShouldSuccessfullyProject_UlidToAnonymousAndDtoTypes(UlidStorageFormat format)
    {
	    // Arrange
	    await using var context = CreateContext(format);
	    var targetUlid = Ulid.New();

	    context.TestEntities.Add(new() { SystemUlid = targetUlid, NullableUlid = Ulid.New() });
	    await context.SaveChangesAsync();
	    context.ChangeTracker.Clear();

	    // Act - Step 1: Project into an anonymous type
	    var anonymousResult = await context.TestEntities
		    .Select(e => new { e.Id, e.SystemUlid, e.NullableUlid })
		    .FirstOrDefaultAsync(e => e.SystemUlid == targetUlid);

	    // Act - Step 2: Project directly into a raw primitive/value type collection
	    var rawUlidList = await context.TestEntities
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
    public async Task EFCore_ShouldTranslateContainsQuery_WhenUsingUlidCollections(UlidStorageFormat format)
    {
	    // Arrange
	    await using var context = CreateContext(format);

	    var targetUlid = Ulid.New();

	    context.TestEntities.AddRange(
		    new TestEntity { SystemUlid = Ulid.New(), NullableUlid = targetUlid },
		    new TestEntity { SystemUlid = Ulid.New(), NullableUlid = null }
	    );
	    await context.SaveChangesAsync();
	    context.ChangeTracker.Clear();

	    // Strategy: Include null directly inside the searchable target criteria collection
	    var searchCriteria = new Ulid?[] { targetUlid, null };

	    // Act
	    var results = await context.TestEntities
		    .Where(e => searchCriteria.Contains(e.NullableUlid))
		    .ToListAsync();

	    // Assert
	    Assert.Equal(2, results.Count);
	    Assert.Contains(results, e => e.NullableUlid == targetUlid);
	    Assert.Contains(results, e => e.NullableUlid == null);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary, 16)]
    [InlineData(UlidStorageFormat.String, 26)]
    public void SchemaMetadata_ShouldHonorConverterMappingHints_ForStringConfiguration(UlidStorageFormat storageFormat, int expectedSize)
    {
        // Arrange
        using var context = CreateContext(storageFormat);
        var model = context.Model;

        // Act
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType?.FindProperty(nameof(TestEntity.SystemUlid));
        var converter = property?.GetValueConverter();

        // Assert
        Assert.NotNull(converter);

        // Assert that sizes match 26 characters explicitly
        Assert.NotNull(converter.MappingHints);
        Assert.Equal(expectedSize, converter.MappingHints.Size);

        if (storageFormat == UlidStorageFormat.String)
        {
			// Assert that Crockford ASCII mapping optimization remains Non-Unicode (CHAR instead of NCHAR)
	        Assert.False(converter.MappingHints.IsUnicode);
        }
    }

    [Theory]
    [InlineData(UlidStorageFormat.String, "TEXT")] // SQLite uses TEXT for string mapping hints
    [InlineData(UlidStorageFormat.Binary, "BLOB")] // SQLite uses BLOB for binary/byte array
    [InlineData(UlidStorageFormat.Guid, "TEXT")] // SQLite uses TEXT for Guid values
    [InlineData(UlidStorageFormat.SqlServerGuid, "TEXT")] // SQLite uses TEXT for Guid values
    public async Task SchemaCreation_ShouldGenerateCorrectDatabaseColumnTypes(UlidStorageFormat format, string expectedDataType)
    {
	    // Arrange & Act
	    await using var context = CreateContext(format);

	    // Query SQLite master schema table
	    await using var command = _connection.CreateCommand();
	    command.CommandText = "PRAGMA table_info(TestEntities);";

	    await using var reader = await command.ExecuteReaderAsync();

	    var foundSystemUlid = false;
	    while (await reader.ReadAsync())
	    {
		    var columnName = reader.GetString(reader.GetOrdinal("name"));
		    if (columnName != nameof(TestEntity.SystemUlid))
		    {
			    continue;
		    }

		    foundSystemUlid = true;
		    var dataType = reader.GetString(reader.GetOrdinal("type"));

		    // Assert
		    Assert.Equal(expectedDataType, dataType);
	    }

	    Assert.True(foundSystemUlid, "SystemUlid column was not found in schema.");
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    public async Task EFCore_ShouldMaintainChronologicalOrder_WhenOrderingByUlid(UlidStorageFormat format)
    {
	    // Arrange
	    await using var context = CreateContext(format);

	    var first = Ulid.New();
	    await Task.Delay(10); // Ensure timestamp progression if relying on machine clock
	    var second = Ulid.New();
	    await Task.Delay(10);
	    var third = Ulid.New();

	    // Add them completely out of order
	    context.TestEntities.AddRange(
		    new TestEntity { SystemUlid = second },
		    new TestEntity { SystemUlid = third },
		    new TestEntity { SystemUlid = first }
	    );
	    await context.SaveChangesAsync();

	    // Act
	    var orderedList = await context.TestEntities.OrderBy(e => e.SystemUlid).ToListAsync();

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
    public async Task EFCore_ShouldSuccessfullyExecuteJoins_OnUlidProperties(UlidStorageFormat format)
    {
	    // Arrange
	    await using var context = CreateContext(format);

	    var parentUlid = Ulid.New();
	    var parentEntity = new TestEntity { SystemUlid = parentUlid };

	    var childEntity = new RelatedChildEntity
	    {
		    ParentSystemUlid = parentUlid,
		    Description = "Child linked via ULID"
	    };

	    context.TestEntities.Add(parentEntity);
	    context.Set<RelatedChildEntity>().Add(childEntity);
	    await context.SaveChangesAsync();
	    context.ChangeTracker.Clear();

	    // Act - Explicit inner join execution over ULID properties
	    var joinResult = await context.TestEntities
		    .Join(
			    context.RelatedChildren,
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
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task EFCore_ShouldNotDetectFalseChanges_WhenEntityIsLoadedButUnmodified(UlidStorageFormat format)
    {
	    // Arrange
	    var originalUlid = Ulid.New();
	    await using (var setupContext = CreateContext(format))
	    {
		    setupContext.TestEntities.Add(new() { SystemUlid = originalUlid, NullableUlid = Ulid.New() });
		    await setupContext.SaveChangesAsync();
	    }

	    // Act & Assert
	    await using (var trackingContext = CreateContext(format))
	    {
		    // Load the entity completely into memory state tracking
		    var dbEntity = await trackingContext.TestEntities.FirstOrDefaultAsync(e => e.SystemUlid == originalUlid);
		    Assert.NotNull(dbEntity);

		    // Attempt a Save without modifying any structural data properties
		    var affectedRows = await trackingContext.SaveChangesAsync();

		    // Verify that EF Core understands the model is completely clean
		    // (Returns 0 updates executed to the underlying data provider)
		    Assert.Equal(0, affectedRows);
	    }
    }

    public void Dispose()
    {
        // Explicitly tear down the shared in-memory connection
        _connection.Dispose();
    }
}