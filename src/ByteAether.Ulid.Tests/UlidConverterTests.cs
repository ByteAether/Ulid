using System.ComponentModel;

namespace ByteAether.Ulid.Tests;
public class UlidConverterTests
{
	private readonly TypeConverter _ulidTypeDescriptor = TypeDescriptor.GetConverter(typeof(Ulid));

	[Theory]
	[InlineData(typeof(string))]
	[InlineData(typeof(byte[]))]
	[InlineData(typeof(Guid))]
	public void CanConvertFrom_ShouldReturnTrue(Type type)
	{
		Assert.True(_ulidTypeDescriptor.CanConvertFrom(type));
	}

	[Theory]
	[InlineData(typeof(string))]
	[InlineData(typeof(byte[]))]
	[InlineData(typeof(Guid))]
	public void CanConvertTo_ShouldReturnTrue(Type type)
	{
		Assert.True(_ulidTypeDescriptor.CanConvertTo(type));
	}

	[Fact]
	public void ConvertFrom_String()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertFromString(source.ToString());

		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_String()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertToString(source);

		Assert.Equal(source.ToString(), result);
		Assert.Equal(26, source.ToString().Length);
	}

	[Fact]
	public void ConvertFrom_Guid()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertFrom(source.ToGuid());

		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_Guid()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertTo(source, typeof(Guid));

		Assert.Equal(source.ToGuid(), result);
	}

	[Fact]
	public void ConvertFrom_ByteArray()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertFrom(source.ToByteArray());

		Assert.Equal(source, result);
	}

	[Fact]
	public void ConvertTo_ByteArray()
	{
		var source = Ulid.New();
		var result = _ulidTypeDescriptor.ConvertTo(source, typeof(byte[]));

		Assert.Equal(source.ToByteArray(), result);
	}
}
