using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ByteAether.Ulid.Cli;

/// <summary>
/// Represents the inspected breakdown of a ULID.
/// </summary>
public sealed record InspectionResult(
	[property: JsonPropertyName("ulid")]
	string Ulid,
	[property: JsonPropertyName("hex")]
	string Hex,
	[property: JsonPropertyName("time")]
	string Time,
	[property: JsonPropertyName("timeHex")]
	string TimeHex,
	[property: JsonPropertyName("timeIso")]
	string TimeIso,
	[property: JsonPropertyName("timestamp")]
	long Timestamp,
	[property: JsonPropertyName("random")]
	string Random,
	[property: JsonPropertyName("randomHex")]
	string RandomHex
)
{
	/// <summary>
	/// Formats the result as structured, easily greppable key-value lines.
	/// </summary>
	public string ToGreppableText()
	{
		return
			$"Ulid:                {Ulid}\n" +
			$"Hex:                 {Hex}\n" +
			$"Time:                {Time}\n" +
			$"Time (Hex):          {TimeHex}\n" +
			$"Time (ISO 8601):     {TimeIso}\n" +
			$"Timestamp (Unix ms): {Timestamp.ToString(CultureInfo.InvariantCulture)}\n" +
			$"Random:              {Random}\n" +
			$"Random (Hex):        {RandomHex}\n";
	}

	/// <summary>
	/// Attempts to extract a single named component value.
	/// </summary>
	/// <param name="partName">The component name to extract.</param>
	/// <param name="value">When this method returns, contains the component value if found; otherwise, null.</param>
	/// <returns>True if the component was recognized; otherwise, false.</returns>
	public bool TryGetPart(string partName, [NotNullWhen(true)] out string? value)
	{
		var normalized = partName.Trim().ToLowerInvariant().Replace(" ", "");
		value = normalized switch
		{
			"ulid" => Ulid,
			"hex" => Hex,
			"time" => Time,
			"timehex" => TimeHex,
			"timeiso" => TimeIso,
			"timestamp" => Timestamp.ToString(CultureInfo.InvariantCulture),
			"random" => Random,
			"randomhex" => RandomHex,
			_ => null
		};
		return value is not null;
	}
}