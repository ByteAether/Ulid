using Dapper;

namespace ByteAether.Ulid.Dapper;

/// <summary>
/// Provides global registration mechanisms to bind <see cref="Ulid"/> and <see cref="Nullable{T}"/> of <see cref="Ulid"/> types
/// into Dapper's internal type handler pipeline.
/// </summary>
public static class DapperUlid
{
	/// <summary>
	/// Registers global Dapper <see cref="SqlMapper.TypeHandler{T}"/> implementations for handling
	/// <see cref="Ulid"/> and <see cref="Nullable{T}"/> of <see cref="Ulid"/> mappings across all database queries.
	/// </summary>
	/// <param name="storageFormat">
	/// The target storage and serialization strategy to apply when persisting ULID data into database columns.
	/// Defaults to <see cref="UlidStorageFormat.String"/>.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if an invalid or unmapped <see cref="UlidStorageFormat"/> value is supplied.
	/// </exception>
	/// <remarks>
	/// <para>
	/// This method configures Dapper globally via a 1:1 type mapping rule. Because Dapper lacks a per-column or
	/// per-property mapping abstraction, only one structural format can be active for the entire application lifecycle.
	/// Multiple sequential configurations will overwrite preceding registrations.
	/// </para>
	/// <para>
	/// Call this method during your application's bootstrap lifecycle (e.g., within <c>Program.cs</c>)
	/// before executing any queries through your database connections.
	/// </para>
	/// </remarks>
	public static void RegisterUlid(UlidStorageFormat storageFormat = UlidStorageFormat.String)
	{
		SqlMapper.ITypeHandler handler = storageFormat switch
		{
			UlidStorageFormat.Binary => new UlidToBytesHandler(),
			UlidStorageFormat.String => new UlidToStringHandler(),
			UlidStorageFormat.Guid => new UlidToGuidHandler(),
			UlidStorageFormat.SqlServerGuid => new UlidToSqlServerGuidHandler(),
			_ => throw new ArgumentOutOfRangeException(nameof(storageFormat), storageFormat, null)
		};

		SqlMapper.AddTypeHandler(typeof(Ulid), handler);
	}
}