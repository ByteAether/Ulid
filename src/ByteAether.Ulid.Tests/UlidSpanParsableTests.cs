namespace ByteAether.Ulid.Tests;

public class UlidSpanParsableTests
{
	[Fact]
	public void TryParse_ValidString_ReturnsTrue()
	{
		var ulidString = "01ARZ3NDEKTSV4RRFFQ69G5FAV";

		var result = Ulid.TryParse(ulidString, null, out var ulid);

		Assert.True(result);
		Assert.NotEqual(default, ulid);
	}

	[Fact]
	public void TryParse_InvalidString_ReturnsFalse()
	{
		var ulidString = "invalid";

		var result = Ulid.TryParse(ulidString, null, out var ulid);

		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Fact]
	public void TryParse_NullString_ReturnsFalse()
	{
		string? ulidString = null;

		var result = Ulid.TryParse(ulidString, null, out var ulid);

		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Fact]
	public void TryParse_ValidSpan_ReturnsTrue()
	{
		var ulidString = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
		var span = ulidString.AsSpan();

		var result = Ulid.TryParse(span, null, out var ulid);

		Assert.True(result);
		Assert.NotEqual(default, ulid);
	}

	[Fact]
	public void TryParse_InvalidSpan_ReturnsFalse()
	{
		var ulidString = "invalid";
		var span = ulidString.AsSpan();

		var result = Ulid.TryParse(span, null, out var ulid);

		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Fact]
	public void TryParse_EmptySpan_ReturnsFalse()
	{
		var ulidString = "";
		var span = ulidString.AsSpan();

		var result = Ulid.TryParse(span, null, out var ulid);

		Assert.False(result);
		Assert.Equal(default, ulid);
	}
}
