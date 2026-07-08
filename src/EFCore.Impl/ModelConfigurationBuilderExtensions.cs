using Microsoft.EntityFrameworkCore;

namespace ByteAether.Ulid.EntityFrameworkCore;

/// <summary>
///
/// </summary>
public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Injected via ConfigureConventions(ModelConfigurationBuilder configurationBuilder).
    /// </summary>
    public static ModelConfigurationBuilder RegisterUlid(
        this ModelConfigurationBuilder configurationBuilder,
        UlidStorageFormat storageFormat = UlidStorageFormat.String)
    {
	    ArgumentNullException.ThrowIfNull(configurationBuilder);

        switch (storageFormat)
        {
            case UlidStorageFormat.Binary:
                configurationBuilder.Properties<Ulid>().HaveConversion<UlidToBytesConverter>();
                configurationBuilder.Properties<Ulid?>().HaveConversion<UlidToBytesConverter>();
                break;
            case UlidStorageFormat.String:
                configurationBuilder.Properties<Ulid>().HaveConversion<UlidToStringConverter>();
                configurationBuilder.Properties<Ulid?>().HaveConversion<UlidToStringConverter>();
                break;
            case UlidStorageFormat.Guid:
                configurationBuilder.Properties<Ulid>().HaveConversion<UlidToGuidConverter>();
                configurationBuilder.Properties<Ulid?>().HaveConversion<UlidToGuidConverter>();
                break;
            case UlidStorageFormat.SqlServerGuid:
                configurationBuilder.Properties<Ulid>().HaveConversion<UlidToSqlServerGuidConverter>();
                configurationBuilder.Properties<Ulid?>().HaveConversion<UlidToSqlServerGuidConverter>();
                break;
            default:
	            throw new ArgumentOutOfRangeException(nameof(storageFormat), storageFormat, null);
        }

        return configurationBuilder;
    }
}