using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace ByteAether.Ulid.Linq2Db;

public static class MappingSchemaExtensions
{
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
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, UlidShuffler.ToSqlServerGuid(ulid), DataType.Guid));

                mappingSchema.SetConvertExpression<Guid, Ulid>(guid => UlidShuffler.FromSqlServerGuid(guid));
                mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(UlidShuffler.FromSqlServerGuid(new(bytes))));

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