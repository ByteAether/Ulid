namespace ByteAether.Ulid.Dapper.IntegrationTests;

public class TestEntity
{
	public int Id { get; set; }
	public Ulid SystemUlid { get; set; }
	public Ulid? NullableUlid { get; set; }
}

public class RelatedChildEntity
{
	public int Id { get; set; }
	public Ulid ParentSystemUlid { get; set; }
	public string Description { get; set; } = string.Empty;
}