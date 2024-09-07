namespace ByteAether.Ulid.Tests;

public class UlidTests
{
	[Fact]
	public void New_GeneratesUniqueUlid()
	{
		var ulid1 = Ulid.New();
		var ulid2 = Ulid.New();

		Assert.NotEqual(ulid1, ulid2);
	}

	[Fact]
	public void New_NonMonotonic_GeneratesUniqueUlid()
	{
		var ulid1 = Ulid.New(false);
		var ulid2 = Ulid.New(false);

		Assert.NotEqual(ulid1, ulid2);
	}

	[Fact]
	public void New_WithGivenDateTime_GeneratesUniqueUlid()
	{
		var ulid1 = Ulid.New(DateTimeOffset.UtcNow);
		var ulid2 = Ulid.New(DateTimeOffset.UtcNow);

		Assert.NotEqual(ulid1, ulid2);
	}

	[Fact]
	public void ToByteArray_ByteArrayAndBack()
	{
		var ulid = Ulid.New();
		var byteArray = ulid.ToByteArray();
		var ulid2 = Ulid.New(byteArray);

		Assert.Equal(16, byteArray.Length);
		Assert.Equal(ulid, ulid2);
	}

	[Fact]
	public void ToGuid_GuidAndBack()
	{
		var ulid = Ulid.New();
		var guid = ulid.ToGuid();
		var ulid2 = Ulid.New(guid);

		Assert.NotEqual(Guid.Empty, guid);
		Assert.Equal(ulid, ulid2);
	}

	[Fact]
	public void ToString_StringAndBack()
	{
		var ulid = Ulid.New();
		var ulidString = ulid.ToString();
		var ulid2 = Ulid.Parse(ulidString);

		Assert.Equal(26, ulidString.Length);
		Assert.Equal(ulid, ulid2);
	}

	[Fact]
	public void Copy_AreEqual()
	{
		var ulid = Ulid.New();
		var ulid2 = ulid.Copy();

		Assert.Equal(ulid, ulid2);
	}
}
