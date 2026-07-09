using Microsoft.EntityFrameworkCore;

namespace ByteAether.Ulid.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for <see cref="ModelConfigurationBuilder"/> to configure ULID (Universally Unique Lexicographically Sortable Identifier) support in Entity Framework Core.
/// </summary>
public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers value converters to globally map <see cref="Ulid"/> and <see cref="Nullable{Ulid}"/> properties
    /// to a specified database storage format.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder being extended.</param>
    /// <param name="storageFormat">The database storage format to use for ULID properties. Defaults to <see cref="UlidStorageFormat.String"/>.</param>
    /// <returns>The same <see cref="ModelConfigurationBuilder"/> instance so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configurationBuilder"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported <paramref name="storageFormat"/> is provided.</exception>
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