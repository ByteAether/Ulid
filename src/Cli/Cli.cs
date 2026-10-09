using System.Text.Json;
using CommandLine;

namespace ByteAether.Ulid.Cli;

/// <summary>
/// Main entry point for the ULID CLI companion tool.
/// </summary>
public static class Cli
{
	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		WriteIndented = true
	};

	/// <summary>
	/// Runs the CLI tool with the specified command-line arguments and standard I/O streams.
	/// </summary>
	/// <param name="args">The command-line arguments.</param>
	/// <param name="stdout">The output text writer (defaults to <see cref="Console.Out"/>).</param>
	/// <param name="stderr">The error text writer (defaults to <see cref="Console.Error"/>).</param>
	/// <param name="stdin">The input text reader (defaults to <see cref="Console.In"/>).</param>
	/// <returns>Exit code: 0 on success, non-zero on failure.</returns>
	public static int Run(string[] args, TextWriter? stdout = null, TextWriter? stderr = null, TextReader? stdin = null)
	{
		stdout ??= Console.Out;
		stderr ??= Console.Error;
		stdin ??= Console.In;

		using var parser = new Parser(settings =>
		{
			settings.CaseInsensitiveEnumValues = true;
			settings.HelpWriter = stdout;
			settings.AutoHelp = true;
			settings.AutoVersion = true;
		});

		var parserResult = parser.ParseArguments<GenerateOptions>(args);

		return parserResult.MapResult(
			options => Execute(options, stdout, stderr, stdin),
			errors =>
			{
				var errorList = errors.ToList();
				return errorList.Any(e => e is HelpRequestedError or VersionRequestedError)
					? 0
					: 1;
			}
		);
	}

	private static int Execute(GenerateOptions options, TextWriter stdout, TextWriter stderr, TextReader stdin)
	{
		if (!TryNormalizeFormat(options.Format, out var format))
		{
			stderr.WriteLine($"Error: Unknown format '{options.Format}'. Available formats: base32 (default), hex, guid.");
			return 1;
		}

		using var ulids = GetUlidsToInspect(options, stdin).GetEnumerator();
		return ulids.MoveNext()
			? ExecuteInspect(ulids, options, stdout, stderr)
			: ExecuteGenerate(options, format, stdout, stderr);
	}

	private static IEnumerable<string> GetUlidsToInspect(GenerateOptions options, TextReader stdin)
	{
		foreach (var ulid in options.Ulids)
		{
			if (!string.IsNullOrWhiteSpace(ulid))
			{
				yield return ulid.Trim();
			}
		}

		if (!HasPipedInput(stdin))
		{
			yield break;
		}

		while (stdin.ReadLine() is { } line)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				yield return line.Trim();
			}
		}
	}

	private static bool HasPipedInput(TextReader stdin)
	{
		if (!ReferenceEquals(stdin, Console.In))
		{
			return stdin.Peek() >= 0;
		}

		return Console.IsInputRedirected;
	}

	private static int ExecuteGenerate(GenerateOptions options, string format, TextWriter stdout, TextWriter stderr)
	{
		if (options.Count < 1)
		{
			stderr.WriteLine("Error: Count must be at least 1.");
			return 1;
		}

		long? timestampMs = null;

		if (options.Timestamp != null)
		{
			if (!InputParser.TryParseTime(options.Timestamp, out timestampMs, out var timeError))
			{
				stderr.WriteLine($"Error: {timeError}");
				return 1;
			}
		}

		byte[]? randomBytes = null;
		if (options.Random != null)
		{
			if (!InputParser.TryParseRandomBytes(options.Random, out var parsedRb, out var rbError))
			{
				stderr.WriteLine($"Error: {rbError}");
				return 1;
			}

			randomBytes = parsedRb;
		}

		try
		{
			var values = Enumerable.Range(0, options.Count)
				.Select(_ => FormatUlid(Generator.Generate(timestampMs, randomBytes), format));
			if (options.Json)
			{
				stdout.WriteLine(options.Count == 1
					? JsonSerializer.Serialize(values.Single(), _serializerOptions)
					: JsonSerializer.Serialize(values, _serializerOptions));
			}
			else
			{
				foreach (var value in values)
				{
					stdout.WriteLine(value);
				}
			}
		}
		catch (Exception ex)
		{
			stderr.WriteLine($"Error generating ULID: {ex.Message}");
			return 1;
		}

		return 0;
	}

	private static int ExecuteInspect(IEnumerator<string> ulids, GenerateOptions options, TextWriter stdout, TextWriter stderr)
	{
		string? errorMessage = null;

		IEnumerable<InspectionResult> GetResults()
		{
			do
			{
				if (!Inspector.TryInspect(ulids.Current, out var result, out errorMessage))
				{
					yield break;
				}

				yield return result;
			}
			while (ulids.MoveNext());
		}
		var results = GetResults();

		IEnumerable<string>? parts = null;
		if (!string.IsNullOrWhiteSpace(options.Part))
		{
			parts = GetParts();

			IEnumerable<string> GetParts()
			{
				foreach (var result in results)
				{
					if (!result.TryGetPart(options.Part, out var partValue))
					{
						errorMessage = $"Unknown part '{options.Part}'. Available parts: ulid, hex, time, timeHex, timeIso, timestamp, random, randomHex.";
						yield break;
					}

					yield return partValue;
				}
			}
		}

		var output = options.Json
			? parts is null
				? SerializeSingleOrArray(results)
				: SerializeSingleOrArray(parts)
			: string.Join(Environment.NewLine, parts ?? results.Select(r => r.ToGreppableText()));

		if (errorMessage is not null)
		{
			stderr.WriteLine($"Error: {errorMessage}");
			return 1;
		}

		stdout.WriteLine(output);
		return 0;
	}

	private static string SerializeSingleOrArray<T>(IEnumerable<T> values)
	{
		var list = values as IReadOnlyList<T> ?? values.ToList();

		return list.Count switch
		{
			0 => JsonSerializer.Serialize(Array.Empty<T>(), _serializerOptions),
			1 => JsonSerializer.Serialize(list[0], _serializerOptions),
			_ => JsonSerializer.Serialize(list, _serializerOptions)
		};
	}

	private static bool TryNormalizeFormat(string? format, out string normalized)
	{
		normalized = string.IsNullOrWhiteSpace(format) ? "base32" : format.Trim().ToLowerInvariant();
		return normalized is "hex" or "guid" or "base32";
	}

	private static string FormatUlid(Ulid ulid, string format)
		=> format switch
		{
			"hex" => Convert.ToHexString(ulid.ToByteArray()),
			"guid" => ulid.ToGuid().ToString(),
			"base32" => ulid.ToString(),
			_ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported ULID output format.")
		};
}