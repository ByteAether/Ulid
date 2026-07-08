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

	[Fact]
	public void IsValid_GoodByteArray()
	{
		// Arrange
		var validUlidBytes = new byte[16];

		// Act
		var result = Ulid.IsValid(new ReadOnlySpan<byte>(validUlidBytes));

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(15)]
	[InlineData(17)]
	public void IsValid_BadByteArray(int length)
	{
		// Arrange
		var validUlidBytes = new byte[length];

		// Act
		var result = Ulid.IsValid(new ReadOnlySpan<byte>(validUlidBytes));

		// Assert
		Assert.False(result);
	}
}
