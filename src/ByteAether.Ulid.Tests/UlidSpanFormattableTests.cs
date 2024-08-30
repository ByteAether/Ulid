namespace ByteAether.Ulid.Tests;

public class UlidSpanFormattableTests
{
	[Fact]
	public void TryFormat_ValidSpan_ReturnsTrue()
	{
		var ulid = Ulid.New();
		Span<char> span = new char[26];

		var result = ulid.TryFormat(span, out var charsWritten, [], null);

		Assert.True(result);
		Assert.Equal(26, charsWritten);
	}

	[Fact]
	public void TryFormat_InvalidSpan_ReturnsFalse()
	{
		var ulid = Ulid.New();
		Span<char> span = new char[10];

		var result = ulid.TryFormat(span, out var charsWritten, [], null);

		Assert.False(result);
		Assert.Equal(0, charsWritten);
	}

	[Fact]
	public void ToString_ReturnsCorrectString()
	{
		var ulid = Ulid.New();
		var ulidString = ulid.ToString();

		Assert.Equal(26, ulidString.Length);
	}
}
