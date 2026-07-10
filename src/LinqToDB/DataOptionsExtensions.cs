using ByteAether.Ulid.DB.Shared;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace ByteAether.Ulid.LinqToDB;

/// <summary>
/// Provides extension methods for <see cref="DataOptions"/> to configure and register ULID support within LinqToDB.
/// </summary>
public static class DataOptionsExtensions
{
	/// <summary>
	/// Registers custom mapping schemas for the <see cref="Ulid"/> type in LinqToDB based on the specified storage format.
	/// </summary>
	/// <param name="options">The <see cref="DataOptions"/> instance to extend.</param>
	/// <param name="storageFormat">The preferred storage format for saving ULIDs to the database. Defaults to <see cref="UlidStorageFormat.String"/>.</param>
	/// <returns>The modified <see cref="DataOptions"/> instance containing the registered ULID mapping schema.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid or unsupported <see cref="UlidStorageFormat"/> is provided.</exception>
	public static DataOptions RegisterUlid(
        this DataOptions options,
        UlidStorageFormat storageFormat = UlidStorageFormat.String
    )
    {
	    var mappingSchema = new MappingSchema();

        switch (storageFormat)
        {
            case UlidStorageFormat.String:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, ulid.ToString(), DataType.Char));

                mappingSchema.SetDataType(typeof(Ulid), DataType.Char);
                break;

            case UlidStorageFormat.Binary:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, ulid.ToByteArray(), DataType.Binary));
                mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(bytes));

                mappingSchema.SetDataType(typeof(Ulid), DataType.Binary);
                break;

            case UlidStorageFormat.Guid:
	            mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, ulid.ToGuid(), DataType.Guid));

	            mappingSchema.SetConvertExpression<Guid, Ulid>(guid => Ulid.New(guid));
	            mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(new Guid(bytes)));

	            mappingSchema.SetDataType(typeof(Ulid), DataType.Guid);
                break;

            case UlidStorageFormat.SqlServerGuid:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, MsSqlUlidShuffler.ToSqlServerGuid(ulid), DataType.Guid));

                mappingSchema.SetConvertExpression<Guid, Ulid>(guid => MsSqlUlidShuffler.FromSqlServerGuid(guid));
                mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(MsSqlUlidShuffler.FromSqlServerGuid(new(bytes))));

                mappingSchema.SetDataType(typeof(Ulid), DataType.Guid);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(storageFormat), storageFormat, null);
        }

        // We should always be able to parse a ULID from a string
        mappingSchema.SetConvertExpression<string, Ulid>(bytes => Ulid.Parse(bytes));

        mappingSchema.SetScalarType(typeof(Ulid));
        mappingSchema.SetCanBeNull(typeof(Ulid), true);

	    return options.UseAdditionalMappingSchema(mappingSchema);
    }
}