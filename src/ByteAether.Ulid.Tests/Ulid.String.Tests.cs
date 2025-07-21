using System.Text;

namespace ByteAether.Ulid.Tests;
public class UlidStringTests
{
	private static readonly string _goodUlidString = "01F8MECHZX3TBDSZG8P8X7XRMM";

	[Fact]
	public void ToString_ShouldReturnExpectedString()
	{
		// Arrange
		var ulid = Ulid.Parse(_goodUlidString);

		// Act
		var result = ulid.ToString();

		// Assert
		Assert.Equal(_goodUlidString, result);
	}

	[Fact]
	public void Parse_ValidString_ShouldReturnUlid()
	{
		// Act
		var result = Ulid.Parse(_goodUlidString);

		// Assert
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Theory]
	[InlineData("")]
	[InlineData("invalid")]
	[InlineData("asd")]
	[InlineData("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU")]
	[InlineData(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
	[InlineData("2222222222222222222222222222222222222")]
	public void Parse_InvalidString_ShouldThrowFormatException(string inputString)
	{
		Assert.Throws<FormatException>(() => Ulid.Parse(inputString));
		Assert.Throws<FormatException>(() => Ulid.Parse(Encoding.UTF8.GetBytes(inputString)));
		Assert.Throws<FormatException>(() => Ulid.Parse(inputString.AsSpan()));
	}

	[Fact]
	public void TryParse_ValidString_ShouldReturnTrueAndUlid()
	{
		// Act
		var success = Ulid.TryParse(_goodUlidString, null, out var result);

		// Assert
		Assert.True(success);
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Theory]
	[InlineData("")]
	[InlineData("invalid")]
	[InlineData("asd")]
	[InlineData("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU")]
	[InlineData(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
	[InlineData("2222222222222222222222222222222222222")]
	public void TryParse_InvalidString_ShouldReturnFalse(string inputString)
	{
		var result = Ulid.TryParse(inputString, null, out var ulid);
		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	public void TryFormat_Chars_ShouldFormatCorrectly(int extraBytesOnBuffer)
	{
		// Arrange
		var ulid = Ulid.Parse(_goodUlidString);
		Span<char> buffer = stackalloc char[Ulid.UlidStringLength + extraBytesOnBuffer];

		// Act
		var success = ulid.TryFormat(buffer, out var charsWritten, []);

		// Assert
		Assert.True(success);
		Assert.Equal(Ulid.UlidStringLength, charsWritten);
		Assert.Equal(_goodUlidString, buffer.Slice(0, Ulid.UlidStringLength).ToString());
	}

	[Fact]
	public void TryFormat_CharsWithInsufficientBuffer_ShouldReturnFalse()
	{
		// Arrange
		var ulid = Ulid.Parse(_goodUlidString);
		Span<char> buffer = stackalloc char[Ulid.UlidStringLength - 1]; // Insufficient buffer size

		// Act
		var success = ulid.TryFormat(buffer, out var charsWritten, []);

		// Assert
		Assert.False(success);
		Assert.Equal(0, charsWritten);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	public void TryFormat_Bytes_ShouldFormatCorrectly(int extraBytesOnBuffer)
	{
		// Arrange
		var ulid = Ulid.Parse(_goodUlidString);
		Span<byte> buffer = stackalloc byte[Ulid.UlidStringLength + extraBytesOnBuffer];

		// Act
		var success = ulid.TryFormat(buffer, out var charsWritten, []);

		// Assert
		Assert.True(success);
		Assert.Equal(Ulid.UlidStringLength, charsWritten);
		Assert.Equal(Encoding.UTF8.GetBytes(_goodUlidString), buffer.Slice(0, Ulid.UlidStringLength).ToArray());
	}

	[Fact]
	public void TryFormat_BytesWithInsufficientBuffer_ShouldReturnFalse()
	{
		// Arrange
		var ulid = Ulid.Parse(_goodUlidString);
		Span<byte> buffer = stackalloc byte[Ulid.UlidStringLength - 1]; // Insufficient buffer size

		// Act
		var success = ulid.TryFormat(buffer, out var charsWritten, []);

		// Assert
		Assert.False(success);
		Assert.Equal(0, charsWritten);
	}

	[Fact]
	public void Parse_ReadOnlySpanChar_ShouldReturnUlid()
	{
		// Arrange
		var span = _goodUlidString.AsSpan();

		// Act
		var result = Ulid.Parse(span);

		// Assert
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Fact]
	public void Parse_ReadOnlySpanByte_ShouldReturnUlid()
	{
		// Arrange
		var bytes = Encoding.UTF8.GetBytes(_goodUlidString);

		// Act
		var result = Ulid.Parse(bytes.AsSpan());

		// Assert
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Fact]
	public void TryParse_ReadOnlySpanChar_ShouldReturnTrueAndUlid()
	{
		// Arrange
		var span = _goodUlidString.AsSpan();

		// Act
		var success = Ulid.TryParse(span, null, out var result);

		// Assert
		Assert.True(success);
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Theory]
	[InlineData("")]
	[InlineData("invalid")]
	[InlineData("asd")]
	[InlineData("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU")]
	[InlineData(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
	[InlineData("2222222222222222222222222222222222222")]
	public void TryParse_ReadOnlySpanChar_InvalidString_ShouldReturnFalse(string inputString)
	{
		var result = Ulid.TryParse(inputString.AsSpan(), null, out var ulid);
		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Fact]
	public void TryParse_ReadOnlySpanByte_ShouldReturnTrueAndUlid()
	{
		// Arrange
		var bytes = Encoding.UTF8.GetBytes(_goodUlidString);

		// Act
		var success = Ulid.TryParse(bytes.AsSpan(), null, out var result);

		// Assert
		Assert.True(success);
		Assert.Equal(_goodUlidString, result.ToString());
	}

	[Theory]
	[InlineData("")]
	[InlineData("invalid")]
	[InlineData("asd")]
	[InlineData("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU")]
	[InlineData(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,")]
	[InlineData("2222222222222222222222222222222222222")]
	public void TryParse_ReadOnlySpanByte_InvalidInput_ShouldReturnFalse(string inputString)
	{
		var bytes = Encoding.UTF8.GetBytes(inputString);

		var result = Ulid.TryParse(bytes.AsSpan(), null, out var ulid);

		Assert.False(result);
		Assert.Equal(default, ulid);
	}

	[Fact]
	public void ToString_WrongLetters_ShouldReplaceWithCorrect()
	{
		// Crockford's Base32 subtitution test
		var inputChars = new[] { 'O', 'o', 'I', 'i', 'L', 'l', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
		var outputChars = new[] { '0', '0', '1', '1', '1', '1', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

		for (var i = 0; i < inputChars.Length; i++)
		{
			var inputString = $"2222222{inputChars[i]}222222222222222222";
			var outputString = $"2222222{outputChars[i]}222222222222222222";

			var ulid = Ulid.Parse(inputString);
			var resultString = ulid.ToString();

			Assert.Equal(outputString, resultString);
		}
	}
}
