namespace ByteAether.Ulid.EntityFrameworkCore;

/// <summary>
///
/// </summary>
public enum UlidStorageFormat
{
	/// <summary>Stores the ULID as a 26-character Crockford Base32 string (e.g., CHAR(26)). (default)</summary>
	String,

	/// <summary>Stores the ULID as a 16-byte binary array (e.g., BINARY(16)).</summary>
	Binary,

	/// <summary>Stores the ULID as a standard native UUID/Guid (e.g., for PostgreSQL uuid).</summary>
	Guid,

	/// <summary>Stores the ULID as an MSSQL uniqueidentifier, shuffling timestamp bytes to guarantee correct chronological sorting.</summary>
	SqlServerGuid
}