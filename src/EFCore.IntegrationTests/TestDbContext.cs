using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ByteAether.Ulid.EntityFrameworkCore.IntegrationTests;

public class TestEntity
{
	public int Id { get; set; }
	public Ulid SystemUlid { get; set; }
	public Ulid? NullableUlid { get; set; }
}

public class RelatedChildEntity
{
	public int Id { get; set; }
	public Ulid ParentSystemUlid { get; set; } // Foreign Key mapped to the ULID
	public string Description { get; set; } = string.Empty;
}

public class TestDbContext(DbContextOptions<TestDbContext> Options, UlidStorageFormat StorageFormat) : DbContext(Options)
{
	public UlidStorageFormat StorageFormat { get; } = StorageFormat;

	public DbSet<TestEntity> TestEntities => Set<TestEntity>();
	public DbSet<RelatedChildEntity> RelatedChildren => Set<RelatedChildEntity>();

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		// Execute the single-line convention extension being tested
		configurationBuilder.RegisterUlid(StorageFormat);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);

		// Replace the default model caching behavior with our custom parameterized factory
		optionsBuilder.ReplaceService<IModelCacheKeyFactory, TestDbContextCacheKeyFactory>();
	}

	public class TestDbContextCacheKeyFactory : IModelCacheKeyFactory
	{
		public object Create(DbContext context, bool designTime)
		{
			// If it's our test context, include the storage format in the cache signature
			if (context is TestDbContext testContext)
			{
				return (context.GetType(), testContext.StorageFormat, designTime);
			}

			return (context.GetType(), designTime);
		}
	}
}