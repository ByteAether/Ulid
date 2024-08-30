namespace ByteAether.Ulid.Tests;

public class UlidUtf8SpanFormattableTests
{
	[Fact]
	public void TryFormat_ValidSpan_ReturnsTrue()
	{
		var ulid = Ulid.New();
		Span<byte> span = new byte[26];

		var result = ulid.TryFormat(span, out var bytesWritten, [], null);

		Assert.True(result);
		Assert.Equal(26, bytesWritten);
	}

	[Fact]
	public void TryFormat_InvalidSpan_ReturnsFalse()
	{
		var ulid = Ulid.New();
		Span<byte> span = new byte[10];

		var result = ulid.TryFormat(span, out var bytesWritten, [], null);

		Assert.False(result);
		Assert.Equal(0, bytesWritten);
	}
}
