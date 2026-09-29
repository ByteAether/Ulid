using CommandLine;

namespace ByteAether.Ulid.Cli;

/// <summary>
/// Command-line options for generating or inspecting ULIDs.
/// </summary>
[Verb("generate", isDefault: true, HelpText = "Generate one or more Universally Unique Lexicographically Sortable Identifiers (ULIDs).")]
public sealed class GenerateOptions
{
	/// <summary>
	/// Gets or sets ULIDs to inspect. If specified, inspection is assumed.
	/// </summary>
	[Value(0, MetaName = "ulid", Required = false, HelpText = "ULID(s) to inspect. Accepts Crockford Base32, hex, or GUID format. Multiple values may also be piped via stdin, one per line.")]
	public IEnumerable<string> Ulids { get; set; } = [];

	/// <summary>
	/// Gets or sets the timestamp or custom 6 time bytes for the ULID.
	/// </summary>
	[Option('t', "time", Required = false, HelpText = "Timestamp/time bytes for generated ULIDs. Accepts an ISO 8601 date string, Unix epoch milliseconds, 12-character hex time bytes, 10-character Crockford Base32 time bytes, or separated byte values.")]
	public string? Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the custom 10 random bytes.
	/// </summary>
	[Option('r', "random", Required = false, HelpText = "Custom 10 random bytes as a 20-character hex string, 16-character Crockford Base32 string, or separated byte values.")]
	public string? Random { get; set; }

	/// <summary>
	/// Gets or sets the number of ULIDs to generate.
	/// </summary>
	[Option('c', "count", Default = 1, Required = false, HelpText = "Number of ULIDs to generate when no ULID input is provided (default: 1).")]
	public int Count { get; set; } = 1;

	/// <summary>
	/// Gets or sets a value indicating whether to output in JSON format.
	/// </summary>
	[Option('j', "json", Default = false, Required = false, HelpText = "Output result in JSON format.")]
	public bool Json { get; set; }

	/// <summary>
	/// Gets or sets the output format for generated ULIDs.
	/// </summary>
	[Option('f', "format", Required = false, HelpText = "Output format for generated ULIDs: 'base32' (default), 'hex', or 'guid'.")]
	public string? Format { get; set; }

	/// <summary>
	/// Gets or sets the part of inspected ULIDs to output.
	/// </summary>
	[Option('p', "part", Required = false, HelpText = "When inspecting, output only one component: 'ulid', 'hex', 'time', 'timeHex', 'timeIso', 'timestamp', 'random', or 'randomHex'.")]
	public string? Part { get; set; }
}