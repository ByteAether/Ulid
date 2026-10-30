using System.Text;

namespace ByteAether.Ulid.Tests;

public class UlidIsValidTests
{
	[Theory]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3")]
	[InlineData("00000000000000000000000000")]
	[InlineData("oooooooooooooooooooooooooo")]
	[InlineData("7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	public void IsValid_GoodString(string goodString)
	{
		// Act
		var isValid = Ulid.IsValid(goodString);

		// Assert
		Assert.True(isValid);
	}

	[Theory]
	[InlineData("80000000000000000000000000")]
	[InlineData("ZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	[InlineData("")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3A")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MVU")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV@")]
	public void IsValid_BadString(string badString)
	{
		// Act
		var isValid = Ulid.IsValid(badString);

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3")]
	[InlineData("00000000000000000000000000")]
	[InlineData("oooooooooooooooooooooooooo")]
	[InlineData("7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	public void IsValid_GoodStringSpan(string goodString)
	{
		// Act
		var isValid = Ulid.IsValid(goodString.AsSpan());

		// Assert
		Assert.True(isValid);
	}

	[Theory]
	[InlineData("80000000000000000000000000")]
	[InlineData("ZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	[InlineData("")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3A")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MVU")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV@")]
	public void IsValid_BadStringSpan(string badString)
	{
		// Act
		var isValid = Ulid.IsValid(badString.AsSpan());

		// Assert
		Assert.False(isValid);
	}

	[Theory]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3")]
	[InlineData("00000000000000000000000000")]
	[InlineData("oooooooooooooooooooooooooo")]
	[InlineData("7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	public void IsValid_GoodUtf8(string goodString)
	{
		// Act
		var isValid = Ulid.IsValid(Encoding.UTF8.GetBytes(goodString).AsSpan());

		// Assert
		Assert.True(isValid);
	}

	[Theory]
	[InlineData("80000000000000000000000000")]
	[InlineData("ZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
	[InlineData("")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV3A")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MVU")]
	[InlineData("01AN4Z07BY79KA1307SR9X4MV@")]
	[InlineData("01AN4Z07BY79KA1307SR9X4M\u00C0")]
	public void IsValid_BadUtf8(string badString)
	{
		// Act
		var isValid = Ulid.IsValid(Encoding.UTF8.GetBytes(badString).AsSpan());

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void IsValid_BinaryUlidBytes_ShouldReturnFalse()
	{
		// Arrange
		var binaryUlid = Ulid.New().ToByteArray();

		// Act
		var result = Ulid.IsValid(new ReadOnlySpan<byte>(binaryUlid));

		// Assert
		Assert.False(result);
	}
}
