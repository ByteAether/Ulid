using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace ByteAether.Ulid.Linq2Db;

public static class MappingSchemaExtensions
{
    public static MappingSchema RegisterUlid(
        this MappingSchema mappingSchema,
        UlidStorageFormat storageFormat = UlidStorageFormat.String)
    {
        ArgumentNullException.ThrowIfNull(mappingSchema);

        // 1. Register conversions for non-nullable Ulid
        switch (storageFormat)
        {
            case UlidStorageFormat.String:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => new(null, ulid.ToString(), DataType.Char));
                mappingSchema.SetConvertExpression<Ulid?, DataParameter>(u => new(null, u.HasValue ? u.Value.ToString() : null, DataType.Char));
                mappingSchema.SetDataType(typeof(Ulid), DataType.Char);
                break;

            case UlidStorageFormat.Binary:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => DataParameter.Binary(null, ulid.ToByteArray()));
                mappingSchema.SetConvertExpression<Ulid?, DataParameter>(u => DataParameter.Binary(null, u.HasValue ? u.Value.ToByteArray() : null));
                mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(bytes));
                mappingSchema.SetConvertExpression<byte[]?, Ulid?>(b => b == null || b.Length == 0 ? null : Ulid.New(b));
                mappingSchema.SetDataType(typeof(Ulid), DataType.Binary);
                break;

            case UlidStorageFormat.Guid:
	            mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => DataParameter.Guid(null, ulid.ToGuid()));
	            mappingSchema.SetConvertExpression<Ulid?, DataParameter>(u => DataParameter.Guid(null, u.HasValue ? u.Value.ToGuid() : Guid.Empty));
	            mappingSchema.SetConvertExpression<Guid, Ulid>(bytes => Ulid.New(bytes));
	            mappingSchema.SetConvertExpression<Guid?, Ulid?>(b => b == null? null : Ulid.New(b.Value));
	            mappingSchema.SetConvertExpression<byte[], Ulid>(bytes => Ulid.New(new Guid(bytes)));
	            mappingSchema.SetConvertExpression<byte[]?, Ulid?>(bytes => bytes == null || bytes.Length == 0 ? null : Ulid.New(new Guid(bytes)));
	            mappingSchema.SetDataType(typeof(Ulid), DataType.Guid);
                break;

            case UlidStorageFormat.SqlServerGuid:
                mappingSchema.SetConvertExpression<Ulid, DataParameter>(ulid => DataParameter.Guid(null, UlidShuffler.ToSqlServerGuid(ulid)));
                mappingSchema.SetConvertExpression<Ulid?, DataParameter>(u => DataParameter.Guid(null, u.HasValue ? UlidShuffler.ToSqlServerGuid(u.Value) : Guid.Empty));
                mappingSchema.SetConvertExpression<Guid, Ulid>(bytes => UlidShuffler.FromSqlServerGuid(bytes));
                mappingSchema.SetConvertExpression<Guid?, Ulid?>(b => b == null ? null : UlidShuffler.FromSqlServerGuid(b.Value));
                mappingSchema.SetDataType(typeof(Ulid), DataType.Guid);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(storageFormat), storageFormat, null);
        }

        mappingSchema.SetConvertExpression<object, Ulid>(obj =>
	        obj.GetType() == typeof(Guid) ? Ulid.New((Guid)obj) :
	        obj.GetType() == typeof(byte[]) ? Ulid.New((byte[])obj) :
	        obj.GetType() == typeof(string) ? Ulid.Parse((string)obj, null) :
	        Ulid.New(Guid.Parse(obj.ToString()!))
	    );

	    mappingSchema.SetConvertExpression<object?, Ulid?>(obj =>
		    obj == null ? null :
		    obj.GetType() == typeof(Guid) ? Ulid.New((Guid)obj) :
		    obj.GetType() == typeof(byte[]) ? Ulid.New((byte[])obj) :
		    obj.GetType() == typeof(string) ? Ulid.Parse((string)obj, null) :
		    Ulid.New(Guid.Parse(obj.ToString()!))
	    );


        /*mappingSchema.SetScalarType(typeof(Ulid));
        mappingSchema.SetCanBeNull(typeof(Ulid), true);*/

        mappingSchema.SetConvertExpression<string, Ulid>(bytes => Ulid.Parse(bytes));
        mappingSchema.SetConvertExpression<string?, Ulid?>(b => b == null || b.Length == 0 ? null : Ulid.Parse(b));



        // 2. Register explicit Nullable<Ulid> handling to bypass fallback expression generation
        //RegisterNullableConversions(mappingSchema, storageFormat);

        return mappingSchema;
    }

    private static void RegisterNullableConversions(MappingSchema schema, UlidStorageFormat format)
    {
        schema.SetDataType(typeof(Ulid?), schema.GetDataType(typeof(Ulid)));

        switch (format)
        {
            case UlidStorageFormat.String:
                schema.SetConvertExpression<Ulid?, string?>(u => u.HasValue ? u.Value.ToString() : null);
                schema.SetConvertExpression<string?, Ulid?>(s => string.IsNullOrEmpty(s) ? null : Ulid.Parse(s, null));
                break;

            case UlidStorageFormat.Binary:

                break;

            case UlidStorageFormat.Guid:
                schema.SetConvertExpression<Ulid?, Guid?>(u => u.HasValue ? u.Value.ToGuid() : null);
                schema.SetConvertExpression<Guid?, Ulid?>(g => g == null || g == Guid.Empty ? null : Ulid.New(g.Value));
                break;

            case UlidStorageFormat.SqlServerGuid:
                schema.SetConvertExpression<Ulid?, Guid?>(u => u.HasValue ? UlidShuffler.ToSqlServerGuid(u.Value) : null);
                schema.SetConvertExpression<Guid?, Ulid?>(g => g == null || g == Guid.Empty ? null : UlidShuffler.FromSqlServerGuid(g.Value));
                break;
        }
    }
}