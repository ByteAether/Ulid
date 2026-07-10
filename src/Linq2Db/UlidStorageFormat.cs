namespace ByteAether.Ulid.Linq2Db;

public enum UlidStorageFormat
{
	/// <summary>Stores the ULID as a 26-character Crockford Base32 string (CHAR(26)).</summary>
	String,

	/// <summary>Stores the ULID as a 16-byte binary array (BINARY(16)).</summary>
	Binary,

	/// <summary>Stores the ULID as a standard native UUID/Guid (PostgreSQL uuid).</summary>
	Guid,

	/// <summary>Stores the ULID as an MSSQL uniqueidentifier with shuffled bytes for chronological sorting.</summary>
	SqlServerGuid
}