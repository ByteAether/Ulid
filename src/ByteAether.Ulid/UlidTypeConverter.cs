using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ByteAether.Ulid;

/// <summary>
/// A type converter for the <see cref="Ulid"/> type.
/// </summary>
public class UlidTypeConverter : TypeConverter
{
	private static readonly Type[] _convertibleTypes = [typeof(string), typeof(byte[]), typeof(Guid)];

	/// <inheritdoc />
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> _convertibleTypes.Contains(sourceType)
			|| base.CanConvertFrom(context, sourceType);

	/// <inheritdoc />
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		=> value switch
		{
			string s => Ulid.Parse(s),
			byte[] b => Ulid.New(b),
			Guid guid => Ulid.New(guid),
			_ => base.ConvertFrom(context, culture, value),
		};

	/// <inheritdoc />
	public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
		=> _convertibleTypes.Contains(destinationType)
			|| base.CanConvertTo(context, destinationType);

	/// <inheritdoc />
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		=> value is Ulid ulid
			? destinationType == typeof(string) ? ulid.ToString()
			: destinationType == typeof(byte[]) ? ulid.ToByteArray()
			: destinationType == typeof(Guid) ? ulid.ToGuid()
			: base.ConvertTo(context, culture, value, destinationType)
		: base.ConvertTo(context, culture, value, destinationType);
}
