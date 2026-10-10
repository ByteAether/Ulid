using Microsoft.Data.Sqlite;
using Dapper;

namespace ByteAether.Ulid.Dapper.IntegrationTests;

public class DapperUlidTypeHandlerRegistrationTests : IDisposable
{
	private readonly SqliteConnection _connection;

	public DapperUlidTypeHandlerRegistrationTests()
	{
		_connection = new("Data Source=:memory:");
		_connection.Open();
	}

	[Fact]
	public async Task RegisterUlid_ShouldRegisterHandler_ForNullableUlidParametersAndResults()
	{
		// Arrange
		SqlMapper.ResetTypeHandlers();
		DapperUlid.RegisterUlid(UlidStorageFormat.String);

		Ulid? value = Ulid.New();

		// Act - A bare scalar query exercises Dapper's parameter-building and result-materialization
		// pipeline directly against a `Ulid?`-typed parameter/result, without going through an entity
		// or a table. This pins that `RegisterUlid` makes `Nullable<Ulid>` resolve to the same
		// type handler as `Ulid`, instead of relying on incidental coverage from the broader
		// entity round-trip tests.
		var roundTripped = await _connection.ExecuteScalarAsync<Ulid?>("SELECT @Value AS Value", new { Value = value });
		var roundTrippedNull = await _connection.ExecuteScalarAsync<Ulid?>("SELECT @Value AS Value", new { Value = (Ulid?)null });

		// Assert
		Assert.Equal(value, roundTripped);
		Assert.Null(roundTrippedNull);
	}

	public void Dispose()
	{
		_connection.Dispose();
		GC.SuppressFinalize(this);
	}
}
