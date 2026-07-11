using System.Data;
using ByteAether.Ulid.DB.Shared;
using Dapper;

namespace ByteAether.Ulid.Dapper;

internal class UlidToBytesHandler : SqlMapper.TypeHandler<Ulid>
{
	public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.Binary;
        parameter.Size = 16;
        parameter.Value = value.ToByteArray();
    }

    public override Ulid Parse(object value)
    {
        if (value is byte[] bytes)
        {
            return Ulid.New(bytes);
        }
        throw new DataException($"Cannot convert {value.GetType().FullName} to Ulid.");
    }
}

internal class UlidToStringHandler : SqlMapper.TypeHandler<Ulid>
{
    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.AnsiStringFixedLength;
        parameter.Size = 26;
        parameter.Value = value.ToString();
    }

    public override Ulid Parse(object value)
    {
        if (value is string str)
        {
            return Ulid.Parse(str);
        }
        throw new DataException($"Cannot convert {value.GetType().FullName} to Ulid.");
    }
}

internal class UlidToGuidHandler : SqlMapper.TypeHandler<Ulid>
{
    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.Guid;
        parameter.Value = value.ToGuid();
    }

    public override Ulid Parse(object value)
    {
	    return value switch
	    {
		    Guid guid => Ulid.New(guid),
		    // Some DBs may store/return GUIDs as strings
		    string str => Ulid.New(Guid.Parse(str)),
		    // Some DBs may store/return GUIDs as byte arrays
		    byte[] bytes => Ulid.New(new Guid(bytes)),
		    _ => throw new DataException($"Cannot convert {value.GetType().FullName} to Ulid.")
	    };
    }
}

internal class UlidToSqlServerGuidHandler : SqlMapper.TypeHandler<Ulid>
{
    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.DbType = DbType.Guid;
        parameter.Value = MsSqlUlidShuffler.ToSqlServerGuid(value);
    }

    public override Ulid Parse(object value)
    {
	    return value switch
	    {
		    Guid guid => MsSqlUlidShuffler.FromSqlServerGuid(guid),
		    // Some DBs may store/return GUIDs as strings
		    string str => MsSqlUlidShuffler.FromSqlServerGuid(Guid.Parse(str)),
		    // Some DBs may store/return GUIDs as byte arrays
		    byte[] bytes => MsSqlUlidShuffler.FromSqlServerGuid(new(bytes)),
		    _ => throw new DataException($"Cannot convert {value.GetType().FullName} to Ulid.")
	    };
    }
}