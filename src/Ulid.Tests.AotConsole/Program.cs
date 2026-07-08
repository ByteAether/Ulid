using System.Text.Json;
using System.Text.Json.Serialization;
using ByteAether.Ulid;

Console.WriteLine("Starting ByteAether.Ulid AOT Compatibility Test...");
Console.WriteLine("--------------------------------------------------");

// --- 1. ULID Creation Tests ---
Console.WriteLine("\n--- ULID Creation ---");

// 1.1 Create a new ULID (default monotonic)
var ulid1_1 = Ulid.New();
Console.WriteLine($"1.1 New ULID (default monotonic): {ulid1_1}");

// 1.2 Create a new non-monotonic ULID
var ulid1_2 = Ulid.New(new Ulid.GenerationOptions { Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic });
Console.WriteLine($"1.2 New ULID (non-monotonic):     {ulid1_2}");

// 1.3 Create a new ULID with MonotonicRandom1Byte
var ulid1_3 = Ulid.New(new Ulid.GenerationOptions { Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte });
Console.WriteLine($"1.3 New ULID (1-byte monotonic):  {ulid1_3}");

// 1.4 Create a new ULID with MonotonicRandom2Byte
var ulid1_4 = Ulid.New(new Ulid.GenerationOptions { Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom2Byte });
Console.WriteLine($"1.4 New ULID (2-byte monotonic):  {ulid1_4}");

// 1.5 Create a new ULID with MonotonicRandom3Byte
var ulid1_5 = Ulid.New(new Ulid.GenerationOptions { Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom3Byte });
Console.WriteLine($"1.5 New ULID (3-byte monotonic):  {ulid1_5}");

// 1.6 Create a new ULID with MonotonicRandom4Byte
var ulid1_6 = Ulid.New(new Ulid.GenerationOptions { Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte });
Console.WriteLine($"1.6 New ULID (4-byte monotonic):  {ulid1_6}");

// 1.7 Create a new ULID from DateTimeOffset
var specificTime = new DateTimeOffset(2023, 10, 26, 10, 30, 0, TimeSpan.Zero);
var ulid1_7 = Ulid.New(specificTime);
Console.WriteLine($"1.7 New ULID from DateTimeOffset: {ulid1_7} (Time: {ulid1_7.Time})");

// 1.8 Create a new ULID from Unix timestamp (long)
var unixTimestamp = specificTime.ToUnixTimeMilliseconds();
var ulid1_8 = Ulid.New(unixTimestamp);
Console.WriteLine($"1.8 New ULID from Unix Timestamp: {ulid1_8} (Time: {ulid1_8.Time})");

// 1.9 Create a new ULID from byte array
var bytes = new byte[16];
new Random().NextBytes(bytes); // Fill with some random bytes
var ulid1_9 = Ulid.New(bytes);
Console.WriteLine($"1.9 New ULID from byte array:     {ulid1_9}");

// 1.10 Create a new ULID from GUID
var guidToConvert = Guid.NewGuid();
var ulid1_10 = Ulid.New(guidToConvert);
Console.WriteLine($"1.10 New ULID from GUID:          {ulid1_10} (Original GUID: {guidToConvert})");

// --- 2. ULID Conversion Tests ---
Console.WriteLine("\n--- ULID Conversion ---");

// 2.1 ToByteArray()
var ulid2_1ByteArray = ulid1_1.ToByteArray();
Console.WriteLine($"2.1 ULID 1 ToByteArray(): {Convert.ToHexString(ulid2_1ByteArray)}");

// 2.2 ToGuid()
var ulid2_2Guid = ulid1_1.ToGuid();
Console.WriteLine($"2.2 ULID 1 ToGuid():      {ulid2_2Guid}");

// 2.3 ToString()
var ulid2_3String = ulid1_1.ToString();
Console.WriteLine($"2.3 ULID 1 ToString():    {ulid2_3String}");

// 2.4 AsByteSpan()
var ulid2_4ByteSpan = ulid1_1.AsByteSpan();
Console.WriteLine($"2.4 ULID 1 AsByteSpan() (first 4 bytes): {Convert.ToHexString(ulid2_4ByteSpan[..4].ToArray())}");

// --- 3. ULID Parsing Tests ---
Console.WriteLine("\n--- ULID Parsing ---");

// 3.1 Parse from string
var ulid3_1 = Ulid.Parse(ulid2_3String);
Console.WriteLine($"3.1 Parsed ULID from string '{ulid2_3String}': {ulid3_1}");
Console.WriteLine($"    Equality check (ulid1_1 == ulid3_1): {ulid1_1 == ulid3_1}");

// 3.2 TryParse from string
if (Ulid.TryParse("01ARZ3NDEKTSV4RRQ6S5KF8XRY", null, out var ulid3_2))
{
    Console.WriteLine($"3.2 TryParse successful: {ulid3_2}");
}
else
{
    throw new("3.2 ERROR: TryParse failed for valid ULID.");
}

// 3.3 TryParse with invalid string
if (!Ulid.TryParse("INVALID_ULID_STRING", null, out var ulid3_3))
{
    Console.WriteLine("3.3 TryParse correctly failed for invalid ULID string.");
}
else
{
	throw new($"3.3 ERROR: TryParse unexpectedly succeeded for invalid ULID string: {ulid3_3}");
}

// --- 4. ULID Validation Tests ---
Console.WriteLine("\n--- ULID Validation ---");

// 4.1 IsValid with valid string
Console.WriteLine($"4.1 IsValid('{ulid2_3String}'): {Ulid.IsValid(ulid2_3String)}");

// 4.2 IsValid with invalid string
Console.WriteLine($"4.2 IsValid('NOT_A_ULID'): {Ulid.IsValid("NOT_A_ULID")}");

// 4.3 IsValid with byte array
Console.WriteLine($"4.3 IsValid(byte[] of ulid1_1): {Ulid.IsValid(ulid2_1ByteArray)}");

// --- 5. ULID Property Access Tests ---
Console.WriteLine("\n--- ULID Property Access ---");

Console.WriteLine($"5.1 ULID 1 Time component:    {ulid1_1.Time}");
Console.WriteLine($"5.2 ULID 1 TimeBytes (first 4 bytes): {Convert.ToHexString(ulid1_1.TimeBytes[..4].ToArray())}");
Console.WriteLine($"5.3 ULID 1 Random (first 4 bytes):    {Convert.ToHexString(ulid1_1.Random[..4].ToArray())}");
Console.WriteLine($"5.4 Ulid.Empty:               {Ulid.Empty}");

// --- 6. Comparison Operator Tests ---
Console.WriteLine("\n--- Comparison Operators ---");
var ulid6_1 = Ulid.New(specificTime);
var ulid6_2 = Ulid.New(specificTime.AddMilliseconds(1));
var ulid6_3 = Ulid.New(specificTime); // Same timestamp, potentially different random

Console.WriteLine($"Compare ULID 1: {ulid6_1}");
Console.WriteLine($"Compare ULID 2: {ulid6_2}");
Console.WriteLine($"Compare ULID 3: {ulid6_3}");

Console.WriteLine($"6.1 ulid6_1 == ulid6_3: {ulid6_1 == ulid6_3}"); // Should be true if random part also matches, depends on generation
Console.WriteLine($"6.2 ulid6_1 != ulid6_2: {ulid6_1 != ulid6_2}");
Console.WriteLine($"6.3 ulid6_1 < ulid6_2:  {ulid6_1 < ulid6_2}");
Console.WriteLine($"6.4 ulid6_1 <= ulid6_3: {ulid6_1 <= ulid6_3}");
Console.WriteLine($"6.5 ulid6_2 > ulid6_1:  {ulid6_2 > ulid6_1}");
Console.WriteLine($"6.6 ulid6_2 >= ulid6_3: {ulid6_2 >= ulid6_3}");

Console.WriteLine($"6.7 CompareTo (ulid1_1 vs ulid1_2): {ulid1_1.CompareTo(ulid1_2)}"); // Should be negative if ulid1_1 is earlier
Console.WriteLine($"6.8 Equals (ulid1_1 vs ulid1_1): {ulid1_1.Equals(ulid1_1)}");
Console.WriteLine($"6.9 Equals (ulid1_1 vs ulid1_2): {ulid1_1.Equals(ulid1_2)}");
Console.WriteLine($"6.10 GetHashCode (ulid1_1): {ulid1_1.GetHashCode()}");

// --- 7. System.Text.Json Integration Test ---
Console.WriteLine("\n--- System.Text.Json Integration (Requires UlidJsonConverter if not using default settings) ---");

// Create an instance of MyClassWithUlid instead of an anonymous type
var ulid7_1Object = new MyClassWithUlid { Id = Ulid.New(), Name = "Test Item" };

// Use the generated TypeInfo from UlidJsonContext for MyClassWithUlid
var jsonString = JsonSerializer.Serialize(ulid7_1Object, UlidJsonContext.Default.MyClassWithUlid);
Console.WriteLine($"7.1 Serialized object with ULID: {jsonString}");

// Deserialize the object using the generated TypeInfo for MyClassWithUlid
var ulid7_2Object = JsonSerializer.Deserialize(jsonString, UlidJsonContext.Default.MyClassWithUlid);
Console.WriteLine($"7.2 Deserialized object ULID: {ulid7_2Object?.Id}, Name: {ulid7_2Object?.Name}");
Console.WriteLine($"    Equality check (Original Id == Deserialized Id): {ulid7_1Object.Id == ulid7_2Object?.Id}");

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