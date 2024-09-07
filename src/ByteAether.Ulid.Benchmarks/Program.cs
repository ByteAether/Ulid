using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run(
	typeof(Program).Assembly,
	ManualConfig
		.Create(DefaultConfig.Instance)
		.WithOptions(ConfigOptions.JoinSummary)
		.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared))
		.WithOptions(ConfigOptions.DisableLogFile)
);

#pragma warning disable CA1822 // Benchmark methods can not be static

[MemoryDiagnoser]
public class Generate
{
	private static readonly NUlid.Rng.IUlidRng _nUlidRandomProvider = new NUlid.Rng.MonotonicUlidRng();

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New();

	[Benchmark]
	public NetUlid.Ulid NetUlid() => global::NetUlid.Ulid.Generate();

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.NewUlid(_nUlidRandomProvider);
}

[MemoryDiagnoser]
public class GenerateNonMono
{
	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(isMonotonic: false);

	[Benchmark]
	public System.Ulid Ulid() => System.Ulid.NewUlid();

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.NewUlid();

	[Benchmark]
	public System.Guid Guid() => System.Guid.NewGuid();
}

[MemoryDiagnoser]
public class ToByteArray
{
	private static readonly byte[] _baseUlidBytes = ByteAether.Ulid.Ulid.New().ToByteArray();

	private static readonly ByteAether.Ulid.Ulid _superUlid = ByteAether.Ulid.Ulid.New(_baseUlidBytes);
	private static readonly NetUlid.Ulid _netUlid = new(_baseUlidBytes);
	private static readonly System.Ulid _ulid = new(_baseUlidBytes);
	private static readonly NUlid.Ulid _nulid = new(_baseUlidBytes);

	[Benchmark]
	public byte[] ByteAetherUlid() => _superUlid.ToByteArray();

	[Benchmark]
	public byte[] NetUlid() => _netUlid.ToByteArray();

	[Benchmark]
	public byte[] Ulid() => _ulid.ToByteArray();

	[Benchmark]
	public byte[] NUlid() => _nulid.ToByteArray();
}

[MemoryDiagnoser]
public class FromByteArray
{
	private static readonly byte[] _baseUlidBytes = ByteAether.Ulid.Ulid.New().ToByteArray();

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(_baseUlidBytes);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => new(_baseUlidBytes);

	[Benchmark]
	public System.Ulid Ulid() => new(_baseUlidBytes);

	[Benchmark]
	public NUlid.Ulid NUlid() => new(_baseUlidBytes);

	[Benchmark]
	public System.Guid Guid() => new(_baseUlidBytes);
}

[MemoryDiagnoser]
public class ToString
{
	private static readonly byte[] _baseUlidBytes = ByteAether.Ulid.Ulid.New().ToByteArray();

	private static readonly ByteAether.Ulid.Ulid _superUlid = ByteAether.Ulid.Ulid.New(_baseUlidBytes);
	private static readonly NetUlid.Ulid _netUlid = new(_baseUlidBytes);
	private static readonly System.Ulid _ulid = new(_baseUlidBytes);
	private static readonly NUlid.Ulid _nulid = new(_baseUlidBytes);
	private static readonly System.Guid _guid = new(_baseUlidBytes);

	[Benchmark]
	public string ByteAetherUlid() => _superUlid.ToString();

	[Benchmark]
	public string NetUlid() => _netUlid.ToString();

	[Benchmark]
	public string Ulid() => _ulid.ToString();

	[Benchmark]
	public string NUlid() => _nulid.ToString();

	[Benchmark]
	public string Guid() => _guid.ToString();
}

[MemoryDiagnoser]
public class FromString
{
	public static ByteAether.Ulid.Ulid _ulid = ByteAether.Ulid.Ulid.New();
	public static string _ulidString = _ulid.ToString();
	public static string _guidString = new System.Guid(_ulid.ToByteArray()).ToString();

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.Parse(_ulidString);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => global::NetUlid.Ulid.Parse(_ulidString);

	[Benchmark]
	public System.Ulid Ulid() => System.Ulid.Parse(_ulidString);

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.Parse(_ulidString);

	[Benchmark]
	public System.Guid Guid() => System.Guid.Parse(_guidString);
}

[MemoryDiagnoser]
public class ToGuid
{
	private static readonly byte[] _baseUlidBytes = ByteAether.Ulid.Ulid.New().ToByteArray();

	private static readonly ByteAether.Ulid.Ulid _superUlid = ByteAether.Ulid.Ulid.New(_baseUlidBytes);
	private static readonly NetUlid.Ulid _netUlid = new(_baseUlidBytes);
	private static readonly System.Ulid _ulid = new(_baseUlidBytes);
	private static readonly NUlid.Ulid _nulid = new(_baseUlidBytes);

	[Benchmark]
	public System.Guid ByteAetherUlid() => _superUlid.ToGuid();

	[Benchmark]
	public System.Guid NetUlid() => new(_netUlid.ToByteArray());

	[Benchmark]
	public System.Guid Ulid() => _ulid.ToGuid();

	[Benchmark]
	public System.Guid NUlid() => _nulid.ToGuid();
}

[MemoryDiagnoser]
public class FromGuid
{
	public static System.Guid _ulidGuid = ByteAether.Ulid.Ulid.New().ToGuid();

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(_ulidGuid);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => new(_ulidGuid.ToByteArray());

	[Benchmark]
	public System.Ulid Ulid() => new(_ulidGuid);

	[Benchmark]
	public NUlid.Ulid NUlid() => new(_ulidGuid);
}

[MemoryDiagnoser]
public class Equals
{
	private static readonly ByteAether.Ulid.Ulid _byteAetherUlid1 = ByteAether.Ulid.Ulid.New();
	private static readonly ByteAether.Ulid.Ulid _byteAetherUlid2 = ByteAether.Ulid.Ulid.New();

	private static readonly NetUlid.Ulid _netUlid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly NetUlid.Ulid _netUlid2 = new(_byteAetherUlid2.ToByteArray());

	private static readonly System.Ulid _ulid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly System.Ulid _ulid2 = new(_byteAetherUlid2.ToByteArray());

	private static readonly NUlid.Ulid _nulid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly NUlid.Ulid _nulid2 = new(_byteAetherUlid2.ToByteArray());

	private static readonly System.Guid _guid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly System.Guid _guid2 = new(_byteAetherUlid2.ToByteArray());

	[Benchmark]
	public bool ByteAetherUlid() => _byteAetherUlid1.Equals(_byteAetherUlid2);

	[Benchmark]
	public bool NetUlid() => _netUlid1.Equals(_netUlid2);

	[Benchmark]
	public bool Ulid() => _ulid1.Equals(_ulid2);

	[Benchmark]
	public bool NUlid() => _nulid1.Equals(_nulid2);

	[Benchmark]
	public bool Guid() => _guid1.Equals(_guid2);
}

[MemoryDiagnoser]
public class CompareTo
{
	private static readonly ByteAether.Ulid.Ulid _byteAetherUlid1 = ByteAether.Ulid.Ulid.New();
	private static readonly ByteAether.Ulid.Ulid _byteAetherUlid2 = ByteAether.Ulid.Ulid.New();

	private static readonly NetUlid.Ulid _netUlid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly NetUlid.Ulid _netUlid2 = new(_byteAetherUlid2.ToByteArray());

	private static readonly System.Ulid _ulid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly System.Ulid _ulid2 = new(_byteAetherUlid2.ToByteArray());

	private static readonly NUlid.Ulid _nulid1 = new(_byteAetherUlid1.ToByteArray());
	private static readonly NUlid.Ulid _nulid2 = new(_byteAetherUlid2.ToByteArray());

	[Benchmark]
	public int ByteAetherUlid() => _byteAetherUlid1.CompareTo(_byteAetherUlid2);

	[Benchmark]
	public int NetUlid() => _netUlid1.CompareTo(_netUlid2);

	[Benchmark]
	public int Ulid() => _ulid1.CompareTo(_ulid2);

	[Benchmark]
	public int NUlid() => _nulid1.CompareTo(_nulid2);
}

[MemoryDiagnoser]
public class GetHashCode
{
	private static readonly byte[] _baseUlidBytes = ByteAether.Ulid.Ulid.New().ToByteArray();

	private static readonly ByteAether.Ulid.Ulid _superUlid = ByteAether.Ulid.Ulid.New(_baseUlidBytes);
	private static readonly NetUlid.Ulid _netUlid = new(_baseUlidBytes);
	private static readonly System.Ulid _ulid = new(_baseUlidBytes);
	private static readonly NUlid.Ulid _nulid = new(_baseUlidBytes);
	private static readonly System.Guid _guid = new(_baseUlidBytes);

	[Benchmark]
	public int ByteAetherUlid() => _superUlid.GetHashCode();

	[Benchmark]
	public int NetUlid() => _netUlid.GetHashCode();

	[Benchmark]
	public int Ulid() => _ulid.GetHashCode();

	[Benchmark]
	public int NUlid() => _nulid.GetHashCode();

	[Benchmark]
	public int Guid() => _guid.GetHashCode();
}