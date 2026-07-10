namespace ByteAether.Ulid.LinqToDB;

/// <summary>
/// Database storage formats for saving <see cref="Ulid"/> properties via LinqToDB.
/// </summary>
public enum UlidStorageFormat
{
	/// <summary>26-character Crockford Base32 string (e.g., CHAR(26)). (default)</summary>
	String,

	/// <summary>16-byte binary array (e.g., BINARY(16)).</summary>
	Binary,

	/// <summary>A standard native UUID/Guid (e.g., for PostgreSQL uuid).</summary>
	Guid,

	/// <summary>An MSSQL uniqueidentifier, shuffling timestamp bytes to guarantee correct chronological sorting.</summary>
	SqlServerGuid
}