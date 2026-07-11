using Microsoft.Data.Sqlite;
using Dapper;

namespace ByteAether.Ulid.Dapper.IntegrationTests;

public class UlidDapperIntegrationTests : IDisposable
{
    private static readonly object _registrationLock = new();
    private readonly SqliteConnection _connection;

    public UlidDapperIntegrationTests()
    {
        // Keep in-memory database alive across queries in the same test pass
        _connection = new("Data Source=:memory:");
        _connection.Open();
    }

    private void PrepareDatabaseAndRegisterFormat(UlidStorageFormat format)
    {
        lock (_registrationLock)
        {
            // Reset Dapper's internal type handler cache entries to force fresh evaluation
            SqlMapper.ResetTypeHandlers();

            // Re-execute our registration method safely under exclusion lock
            DapperUlid.RegisterUlid(format);
        }

        // Drop tables if they exist to provide complete isolation per theory run
        _connection.Execute("DROP TABLE IF EXISTS RelatedChildren;");
        _connection.Execute("DROP TABLE IF EXISTS TestEntities;");

        // Map sqlite column schemas to match format constraints
        var ulidColumnType = format switch
        {
            UlidStorageFormat.Binary => "BLOB",
            UlidStorageFormat.String => "TEXT(26)",
            UlidStorageFormat.Guid => "TEXT(36)",
            UlidStorageFormat.SqlServerGuid => "TEXT(36)",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        _connection.Execute($@"
            CREATE TABLE TestEntities (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SystemUlid {ulidColumnType} NOT NULL,
                NullableUlid {ulidColumnType} NULL
        	);
		");

        _connection.Execute($@"
            CREATE TABLE RelatedChildren (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ParentSystemUlid {ulidColumnType} NOT NULL,
                Description TEXT NOT NULL
            );
		");
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task Dapper_ShouldSuccessfullyRoundTrip_BothNonNullAndNullableUlids(UlidStorageFormat format)
    {
        // Arrange
        PrepareDatabaseAndRegisterFormat(format);
        var originalUlid = Ulid.New();
        var nullableUlid = Ulid.New();

        var entity = new TestEntity
        {
            SystemUlid = originalUlid,
            NullableUlid = nullableUlid
        };

        // Act
        const string insertSql = @"
            INSERT INTO TestEntities (SystemUlid, NullableUlid) 
            VALUES (@SystemUlid, @NullableUlid);
            SELECT last_insert_rowid();
		";

        var assignedId = await _connection.ExecuteScalarAsync<int>(insertSql, entity);

        const string selectSql = "SELECT Id, SystemUlid, NullableUlid FROM TestEntities WHERE Id = @Id;";
        var retrievedEntity = await _connection.QueryFirstOrDefaultAsync<TestEntity>(selectSql, new { Id = assignedId });

        // Assert
        Assert.NotNull(retrievedEntity);
        Assert.Equal(assignedId, retrievedEntity.Id);
        Assert.Equal(originalUlid, retrievedEntity.SystemUlid);
        Assert.Equal(nullableUlid, retrievedEntity.NullableUlid);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task Dapper_ShouldSuccessfullyRoundTrip_WithNullValueInNullableUlid(UlidStorageFormat format)
    {
        // Arrange
        PrepareDatabaseAndRegisterFormat(format);
        var originalUlid = Ulid.New();

        var entity = new TestEntity
        {
            SystemUlid = originalUlid,
            NullableUlid = null
        };

        // Act
        const string insertSql = @"
            INSERT INTO TestEntities (SystemUlid, NullableUlid) 
            VALUES (@SystemUlid, @NullableUlid);
            SELECT last_insert_rowid();
		";

        var assignedId = await _connection.ExecuteScalarAsync<int>(insertSql, entity);

        const string selectSql = "SELECT Id, SystemUlid, NullableUlid FROM TestEntities WHERE Id = @Id;";
        var retrievedEntity = await _connection.QueryFirstOrDefaultAsync<TestEntity>(selectSql, new { Id = assignedId });

        // Assert
        Assert.NotNull(retrievedEntity);
        Assert.Equal(originalUlid, retrievedEntity.SystemUlid);
        Assert.Null(retrievedEntity.NullableUlid);
    }

    [Theory]
    [InlineData(UlidStorageFormat.Binary)]
    [InlineData(UlidStorageFormat.String)]
    [InlineData(UlidStorageFormat.Guid)]
    [InlineData(UlidStorageFormat.SqlServerGuid)]
    public async Task Dapper_ShouldQueryAndJoinSuccessfully_UsingUlidParameters(UlidStorageFormat format)
    {
        // Arrange
        PrepareDatabaseAndRegisterFormat(format);
        var parentUlid = Ulid.New();

        const string insertParentSql = "INSERT INTO TestEntities (SystemUlid, NullableUlid) VALUES (@SystemUlid, NULL);";
        await _connection.ExecuteAsync(insertParentSql, new { SystemUlid = parentUlid });

        const string insertChildSql = "INSERT INTO RelatedChildren (ParentSystemUlid, Description) VALUES (@ParentUlid, @Desc);";
        await _connection.ExecuteAsync(insertChildSql, new { ParentUlid = parentUlid, Desc = "Child linked via ULID" });

        // Act
        const string joinSql = @"
            SELECT p.SystemUlid, c.Description 
            FROM TestEntities p
            INNER JOIN RelatedChildren c ON p.SystemUlid = c.ParentSystemUlid
            WHERE p.SystemUlid = @TargetUlid;
		";

        var results = await _connection.QueryAsync<(Ulid SystemUlid, string Description)>(joinSql, new { TargetUlid = parentUlid });
        var joinResult = results.FirstOrDefault();

        // Assert
        Assert.Equal(parentUlid, joinResult.SystemUlid);
        Assert.Equal("Child linked via ULID", joinResult.Description);
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}