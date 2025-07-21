using System.Text.Json;
using System.Text.Json.Serialization;
using ByteAether.Ulid;

Console.WriteLine("Starting ByteAether.Ulid AOT Compatibility Test...");
Console.WriteLine("--------------------------------------------------");

// --- 1. ULID Creation Tests ---
Console.WriteLine("\n--- ULID Creation ---");

// 1.1 Create a new ULID (default monotonic)
var ulid1 = Ulid.New();
Console.WriteLine($"1.1 New ULID (default monotonic): {ulid1}");

// 1.2 Create a new non-monotonic ULID
var ulid2 = Ulid.New(isMonotonic: false);
Console.WriteLine($"1.2 New ULID (non-monotonic):     {ulid2}");

// 1.3 Create a new ULID from DateTimeOffset
var specificTime = new DateTimeOffset(2023, 10, 26, 10, 30, 0, TimeSpan.Zero);
var ulid3 = Ulid.New(specificTime);
Console.WriteLine($"1.3 New ULID from DateTimeOffset: {ulid3} (Time: {ulid3.Time})");

// 1.4 Create a new ULID from Unix timestamp (long)
var unixTimestamp = specificTime.ToUnixTimeMilliseconds();
var ulid4 = Ulid.New(unixTimestamp);
Console.WriteLine($"1.4 New ULID from Unix Timestamp: {ulid4} (Time: {ulid4.Time})");

// 1.5 Create a new ULID from byte array
var bytes = new byte[16];
new Random().NextBytes(bytes); // Fill with some random bytes
var ulid5 = Ulid.New(bytes);
Console.WriteLine($"1.5 New ULID from byte array:     {ulid5}");

// 1.6 Create a new ULID from GUID
var guidToConvert = Guid.NewGuid();
var ulid6 = Ulid.New(guidToConvert);
Console.WriteLine($"1.6 New ULID from GUID:           {ulid6} (Original GUID: {guidToConvert})");

// --- 2. ULID Conversion Tests ---
Console.WriteLine("\n--- ULID Conversion ---");

// 2.1 ToByteArray()
var ulid1ByteArray = ulid1.ToByteArray();
Console.WriteLine($"2.1 ULID 1 ToByteArray(): {Convert.ToHexString(ulid1ByteArray)}");

// 2.2 ToGuid()
var ulid1Guid = ulid1.ToGuid();
Console.WriteLine($"2.2 ULID 1 ToGuid():      {ulid1Guid}");

// 2.3 ToString()
var ulid1String = ulid1.ToString();
Console.WriteLine($"2.3 ULID 1 ToString():    {ulid1String}");

// 2.4 AsByteSpan()
var ulid1ByteSpan = ulid1.AsByteSpan();
Console.WriteLine($"2.4 ULID 1 AsByteSpan() (first 4 bytes): {Convert.ToHexString(ulid1ByteSpan[..4].ToArray())}");


// --- 3. ULID Parsing Tests ---
Console.WriteLine("\n--- ULID Parsing ---");

// 3.1 Parse from string
var parsedUlid1 = Ulid.Parse(ulid1String);
Console.WriteLine($"3.1 Parsed ULID from string '{ulid1String}': {parsedUlid1}");
Console.WriteLine($"    Equality check (ulid1 == parsedUlid1): {ulid1 == parsedUlid1}");

// 3.2 TryParse from string
if (Ulid.TryParse("01ARZ3NDEKTSV4RRQ6S5KF8XRY", null, out var tryParsedUlid))
{
	Console.WriteLine($"3.2 TryParse successful: {tryParsedUlid}");
}
else
{
	Console.WriteLine("3.2 TryParse failed for valid ULID.");
}

// 3.3 TryParse with invalid string
if (!Ulid.TryParse("INVALID_ULID_STRING", null, out var failedParseUlid))
{
	Console.WriteLine("3.3 TryParse correctly failed for invalid ULID string.");
}
else
{
	Console.WriteLine($"3.3 TryParse unexpectedly succeeded for invalid ULID string: {failedParseUlid}");
}

// --- 4. ULID Validation Tests ---
Console.WriteLine("\n--- ULID Validation ---");

// 4.1 IsValid with valid string
Console.WriteLine($"4.1 IsValid('{ulid1String}'): {Ulid.IsValid(ulid1String)}");

// 4.2 IsValid with invalid string
Console.WriteLine($"4.2 IsValid('NOT_A_ULID'): {Ulid.IsValid("NOT_A_ULID")}");

// 4.3 IsValid with byte array
Console.WriteLine($"4.3 IsValid(byte[] of ulid1): {Ulid.IsValid(ulid1ByteArray)}");


// --- 5. ULID Property Access Tests ---
Console.WriteLine("\n--- ULID Property Access ---");

Console.WriteLine($"5.1 ULID 1 Time component:    {ulid1.Time}");
Console.WriteLine($"5.2 ULID 1 TimeBytes (first 4 bytes): {Convert.ToHexString(ulid1.TimeBytes[..4].ToArray())}");
Console.WriteLine($"5.3 ULID 1 Random (first 4 bytes):    {Convert.ToHexString(ulid1.Random[..4].ToArray())}");
Console.WriteLine($"5.4 Ulid.Empty:               {Ulid.Empty}");


// --- 6. Comparison Operator Tests ---
Console.WriteLine("\n--- Comparison Operators ---");
var compareUlid1 = Ulid.New(specificTime);
var compareUlid2 = Ulid.New(specificTime.AddMilliseconds(1));
var compareUlid3 = Ulid.New(specificTime); // Same timestamp, potentially different random

Console.WriteLine($"Compare ULID 1: {compareUlid1}");
Console.WriteLine($"Compare ULID 2: {compareUlid2}");
Console.WriteLine($"Compare ULID 3: {compareUlid3}");

Console.WriteLine($"6.1 compareUlid1 == compareUlid3: {compareUlid1 == compareUlid3}"); // Should be true if random part also matches, depends on generation
Console.WriteLine($"6.2 compareUlid1 != compareUlid2: {compareUlid1 != compareUlid2}");
Console.WriteLine($"6.3 compareUlid1 < compareUlid2:  {compareUlid1 < compareUlid2}");
Console.WriteLine($"6.4 compareUlid1 <= compareUlid3: {compareUlid1 <= compareUlid3}");
Console.WriteLine($"6.5 compareUlid2 > compareUlid1:  {compareUlid2 > compareUlid1}");
Console.WriteLine($"6.6 compareUlid2 >= compareUlid3: {compareUlid2 >= compareUlid3}");

Console.WriteLine($"6.7 CompareTo (ulid1 vs ulid2): {ulid1.CompareTo(ulid2)}"); // Should be negative if ulid1 is earlier
Console.WriteLine($"6.8 Equals (ulid1 vs ulid1): {ulid1.Equals(ulid1)}");
Console.WriteLine($"6.9 Equals (ulid1 vs ulid2): {ulid1.Equals(ulid2)}");
Console.WriteLine($"6.10 GetHashCode (ulid1): {ulid1.GetHashCode()}");

// --- 7. System.Text.Json Integration Test ---
Console.WriteLine("\n--- System.Text.Json Integration (Requires UlidJsonConverter if not using default settings) ---");

// Create an instance of MyClassWithUlid instead of an anonymous type
var myObject = new MyClassWithUlid { Id = Ulid.New(), Name = "Test Item" };

// Use the generated TypeInfo from UlidJsonContext for MyClassWithUlid
var jsonString = JsonSerializer.Serialize(myObject, UlidJsonContext.Default.MyClassWithUlid);
Console.WriteLine($"7.1 Serialized object with ULID: {jsonString}");

// Deserialize the object using the generated TypeInfo for MyClassWithUlid
var deserializedObject = JsonSerializer.Deserialize(jsonString, UlidJsonContext.Default.MyClassWithUlid);
Console.WriteLine($"7.2 Deserialized object ULID: {deserializedObject?.Id}, Name: {deserializedObject?.Name}");
Console.WriteLine($"    Equality check (Original Id == Deserialized Id): {myObject.Id == deserializedObject?.Id}");

Console.WriteLine("\n--------------------------------------------------");
Console.WriteLine("ByteAether.Ulid AOT Compatibility Test Completed.");

internal class MyClassWithUlid
{
	public Ulid Id { get; set; }
	public string? Name { get; set; }
}

[JsonSerializable(typeof(Ulid))]
[JsonSerializable(typeof(MyClassWithUlid))]
internal partial class UlidJsonContext : JsonSerializerContext
{ }