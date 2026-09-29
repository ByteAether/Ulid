namespace ByteAether.Ulid.Cli.Tests;

public class CliTests
{
	[Fact]
	public void Run_NoArgs_GeneratesSingleUlid()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run([], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		var output = stdout.ToString().Trim();
		Assert.Equal(26, output.Length);
		Assert.True(Ulid.TryParse(output, null, out _));
	}

	[Fact]
	public void Run_GenerateCount3_Generates3Lines()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["-c", "3"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		var lines = stdout.ToString().Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
		Assert.Equal(3, lines.Length);
		foreach (var line in lines)
		{
			Assert.True(Ulid.TryParse(line.Trim(), null, out _));
		}
	}

	[Fact]
	public void Run_GenerateWithExactBytes_ProducesExpectedUlid()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run([
			"--time", "01A0ED093A00",
			"--random", "5D6762477ACBF9130587"
		], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.Equal("01M3PGJEG0BNKP4HVTSFWH61C7", stdout.ToString().Trim());
	}

	[Fact]
	public void Run_GenerateHexFormat_Produces32CharHex()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run([
			"--time", "01A0ED093A00",
			"--random", "5D6762477ACBF9130587",
			"-f", "hex"
		], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.Equal("01A0ED093A005D6762477ACBF9130587", stdout.ToString().Trim());
	}

	[Fact]
	public void Run_GenerateUnknownFormat_ReturnsErrorInsteadOfDefaulting()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["--format", "binary"], stdout, stderr, stdin);

		Assert.Equal(1, exitCode);
		Assert.Empty(stdout.ToString());
		Assert.Contains("Unknown format 'binary'", stderr.ToString());
	}

	[Fact]
	public void Run_GenerateJsonBatch_StreamsValidJsonArray()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["--count", "3", "--json"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		using var document = System.Text.Json.JsonDocument.Parse(stdout.ToString());
		Assert.Equal(3, document.RootElement.GetArrayLength());
		foreach (var item in document.RootElement.EnumerateArray())
		{
			Assert.True(Ulid.TryParse(item.GetString(), null, out _));
		}
	}

	[Fact]
	public void Run_Inspect_OutputsExpectedGreppableLines()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		var output = stdout.ToString();
		Assert.Contains("Ulid:                01M3PGJEG0BNKP4HVTSFWH61C7", output);
		Assert.Contains("Hex:                 01A0ED093A005D6762477ACBF9130587", output);
		Assert.Contains("Time:                01M3PGJEG0", output);
		Assert.Contains("Time (Hex):          01A0ED093A00", output);
		Assert.Contains("Random:              BNKP4HVTSFWH61C7", output);
		Assert.Contains("Random (Hex):        5D6762477ACBF9130587", output);
	}

	[Fact]
	public void Run_InspectPartTimeHex_OutputsOnlyTimeHex()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "-p", "timeHex"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.Equal("01A0ED093A00", stdout.ToString().Trim());
	}

	[Fact]
	public void Run_InspectPartRandomCrockford_OutputsOnlyRandomC32()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "-p", "random"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.Equal("BNKP4HVTSFWH61C7", stdout.ToString().Trim());
	}

	[Theory]
	[InlineData("ulid", "01M3PGJEG0BNKP4HVTSFWH61C7")]
	[InlineData("hex", "01A0ED093A005D6762477ACBF9130587")]
	[InlineData("time", "01M3PGJEG0")]
	[InlineData("timeHex", "01A0ED093A00")]
	[InlineData("timeIso", "2026-09-29T12:00:00.000Z")]
	[InlineData("timestamp", "1790683200000")]
	[InlineData("random", "BNKP4HVTSFWH61C7")]
	[InlineData("randomHex", "5D6762477ACBF9130587")]
	public void Run_InspectPart_OutputsDocumentedValue(string part, string expected)
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "--part", part], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.Equal(expected, stdout.ToString().Trim());
	}

	[Fact]
	public void Run_InspectPartJson_OutputsJsonString()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "--part", "timeHex", "--json"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		using var document = System.Text.Json.JsonDocument.Parse(stdout.ToString());
		Assert.Equal(System.Text.Json.JsonValueKind.String, document.RootElement.ValueKind);
		Assert.Equal("01A0ED093A00", document.RootElement.GetString());
	}

	[Fact]
	public void Run_InspectMultiplePartJsonInputs_OutputsJsonArray()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("01M3PGJEG0BNKP4HVTSFWH61C7\n01M3PGJEG0BNKP4HVTSFWH61C7\n");

		var exitCode = Cli.Run(["--part", "random", "--json"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		using var document = System.Text.Json.JsonDocument.Parse(stdout.ToString());
		Assert.Equal(System.Text.Json.JsonValueKind.Array, document.RootElement.ValueKind);
		Assert.Equal(2, document.RootElement.GetArrayLength());
		Assert.All(document.RootElement.EnumerateArray(), item => Assert.Equal("BNKP4HVTSFWH61C7", item.GetString()));
	}

	[Fact]
	public void Run_InspectPartUndocumentedName_ReturnsError()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "--part", "time-c32"], stdout, stderr, stdin);

		Assert.Equal(1, exitCode);
		Assert.Empty(stdout.ToString());
		Assert.Contains("Available parts: ulid, hex, time, timeHex, timeIso, timestamp, random, randomHex", stderr.ToString());
	}

	[Fact]
	public void Run_InspectMultipleJsonInputs_OutputsValidJsonArray()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("01M3PGJEG0BNKP4HVTSFWH61C7\n01M3PGJEG0BNKP4HVTSFWH61C7\n");

		var exitCode = Cli.Run(["--json"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		using var document = System.Text.Json.JsonDocument.Parse(stdout.ToString());
		Assert.Equal(2, document.RootElement.GetArrayLength());
	}

	[Fact]
	public void Run_InspectInvalidLaterJsonInput_DoesNotWritePartialJson()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("01M3PGJEG0BNKP4HVTSFWH61C7\ninvalid\n");

		var exitCode = Cli.Run(["--json"], stdout, stderr, stdin);

		Assert.Equal(1, exitCode);
		Assert.Empty(stdout.ToString());
		Assert.Contains("not a valid ULID", stderr.ToString());
	}

	[Fact]
	public void Run_InspectInvalidLaterTextInput_DoesNotWritePartialOutput()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("01M3PGJEG0BNKP4HVTSFWH61C7\ninvalid\n");

		var exitCode = Cli.Run([], stdout, stderr, stdin);

		Assert.Equal(1, exitCode);
		Assert.Empty(stdout.ToString());
		Assert.Contains("not a valid ULID", stderr.ToString());
	}

	[Fact]
	public void Run_InspectJson_OutputsValidJson()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["01M3PGJEG0BNKP4HVTSFWH61C7", "--json"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		var json = stdout.ToString().Trim();
		Assert.StartsWith("{", json);
		Assert.Contains("\"timeHex\": \"01A0ED093A00\"", json);
	}

	[Fact]
	public void Run_InspectInvalidUlid_ReturnsExitCode1()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["invalid_id"], stdout, stderr, stdin);

		Assert.Equal(1, exitCode);
		Assert.Contains("not a valid ULID", stderr.ToString());
	}

	[Fact]
	public void Run_Help_ReturnsExitCode0AndPrintsHelp()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["--help"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		var output = stdout.ToString();
		Assert.Contains("generate", output);
		Assert.Contains("inspect", output);
	}

	[Fact]
	public void Run_Version_ReturnsExitCode0AndPrintsVersion()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["--version"], stdout, stderr, stdin);

		Assert.Equal(0, exitCode);
		Assert.NotEqual(string.Empty, stdout.ToString().Trim());
	}

	[Fact]
	public void Run_UnknownArgument_ReturnsNonZeroExitCode()
	{
		using var stdout = new StringWriter();
		using var stderr = new StringWriter();
		using var stdin = new StringReader("");

		var exitCode = Cli.Run(["--unknown-flag-test"], stdout, stderr, stdin);

		Assert.NotEqual(0, exitCode);
	}
}