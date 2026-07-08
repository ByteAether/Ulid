using System.ComponentModel;

namespace ByteAether.Ulid.Tests;

public class UlidTypeConverterTests
{
	private readonly TypeConverter _ulidTypeDescriptor = TypeDescriptor.GetConverter(typeof(Ulid));

	[Theory]
	[InlineData(typeof(string))]
	[InlineData(typeof(byte[]))]
	[InlineData(typeof(Guid))]
	public void CanConvertFrom_ShouldReturnTrueForSupportedTypes(Type type)
	{
		// Act
		var result = _ulidTypeDescriptor.CanConvertFrom(type);

		// Assert
		Assert.True(result, $"TypeConverter should support converting from {type}");
	}

	[Fact]
	public void CanConvertFrom_ShouldReturnFalseForUnsupportedType()
	{
		// Act
		var result = _ulidTypeDescriptor.CanConvertFrom(typeof(object));

		// Assert
		Assert.False(result, $"TypeConverter should not support converting from {typeof(object)}");
	}

	[Theory]
	[InlineData(typeof(string))]
	[InlineData(typeof(byte[]))]
	[InlineData(typeof(Guid))]
	public void CanConvertTo_ShouldReturnTrueForSupportedTypes(Type type)
	{
		// Act
		var result = _ulidTypeDescriptor.CanConvertTo(type);

		// Assert
		Assert.True(result, $"TypeConverter should support converting to {type}");
	}

	[Fact]
	public void CanConvertTo_ShouldReturnFalseForUnsupportedType()
	{
		// Act
		var result = _ulidTypeDescriptor.CanConvertTo(typeof(object));

		// Assert
		Assert.False(result, $"TypeConverter should not support converting to {typeof(object)}");
	}

	[Fact]
	public void ConvertFrom_ShouldConvertFromString()
	{
		// Arrange
		var source = Ulid.New();
		var sourceString = source.ToString();

		// Act
		var result = _ulidTypeDescriptor.ConvertFromString(sourceString);

		// Assert
		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_ShouldConvertToString()
	{
		// Arrange
		var source = Ulid.New();

		// Act
		var result = _ulidTypeDescriptor.ConvertToString(source);

		// Assert
		Assert.Equal(source.ToString(), result);
		Assert.Equal(26, source.ToString().Length); // ULID string representation length is always 26
	}

	[Fact]
	public void ConvertFrom_ShouldConvertFromGuid()
	{
		// Arrange
		var source = Ulid.New();
		var sourceGuid = source.ToGuid();

		// Act
		var result = _ulidTypeDescriptor.ConvertFrom(sourceGuid);

		// Assert
		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_ShouldConvertToGuid()
	{
		// Arrange
		var source = Ulid.New();

		// Act
		var result = _ulidTypeDescriptor.ConvertTo(source, typeof(Guid));

		// Assert
		Assert.Equal(source.ToGuid(), result);
	}

	[Fact]
	public void ConvertFrom_ShouldConvertFromByteArray()
	{
		// Arrange
		var source = Ulid.New();
		var sourceBytes = source.ToByteArray();

		// Act
		var result = _ulidTypeDescriptor.ConvertFrom(sourceBytes);

		// Assert
		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_ShouldConvertToByteArray()
	{
		// Arrange
		var source = Ulid.New();

		// Act
		var result = _ulidTypeDescriptor.ConvertTo(source, typeof(byte[]));

		// Assert
		Assert.Equal(source.ToByteArray(), result);
	}
}
