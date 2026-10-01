namespace ByteAether.Ulid.Cli;

internal static class Hex
{
	public static bool TryParseBytes(string input, int expectedLength, out byte[] bytes)
	{
		var cleaned = Clean(input);
		if (cleaned.Length != expectedLength * 2)
		{
			bytes = [];
			return false;
		}

		try
		{
			bytes = Convert.FromHexString(cleaned);
			return true;
		}
		catch (FormatException)
		{
			bytes = [];
			return false;
		}
	}

	public static string Clean(string input)
	{
		var text = input.Trim();
		if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			text = text[2..];
		}

		return text
			.Replace(" ", "")
			.Replace("-", "")
			.Replace(":", "")
			.Replace(",", "");
	}
}
