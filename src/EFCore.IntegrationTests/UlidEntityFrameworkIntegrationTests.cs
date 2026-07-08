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

        // Act - Step 2: Read back from an isolated context instance
        await using (var readContext = CreateContext(format))
        {
            var dbEntity = await readContext.TestEntities.FirstOrDefaultAsync(e => e.SystemUlid == originalUlid);

            // Assert
            Assert.NotNull(dbEntity);
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
        }
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
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

    [Fact]
    public void SchemaMetadata_ShouldHonorConverterMappingHints_ForStringConfiguration()
    {
        // Arrange
        using var context = CreateContext(UlidStorageFormat.String);
        var model = context.Model;

        // Act
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType?.FindProperty(nameof(TestEntity.SystemUlid));
        var converter = property?.GetValueConverter();

        // Assert
        Assert.NotNull(converter);

        // Assert that sizes match 26 characters explicitly
        Assert.NotNull(converter.MappingHints);
        Assert.Equal(26, converter.MappingHints.Size);

        // Assert that Crockford ASCII mapping optimization remains Non-Unicode (CHAR instead of NCHAR)
        Assert.False(converter.MappingHints.IsUnicode);
    }

    [Fact]
    public void SchemaMetadata_ShouldHonorConverterMappingHints_ForBinaryConfiguration()
    {
        // Arrange
        using var context = CreateContext(UlidStorageFormat.Binary);
        var model = context.Model;

        // Act
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType?.FindProperty(nameof(TestEntity.SystemUlid));
        var converter = property?.GetValueConverter();

        // Assert
        Assert.NotNull(converter);
        Assert.NotNull(converter.MappingHints);

        // Explicitly assert fixed length boundary requirement
        Assert.Equal(16, converter.MappingHints.Size);
    }

    public void Dispose()
    {
        // Explicitly tear down the shared in-memory connection
        _connection.Dispose();
    }
}