using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

var benchmarkConfig = ManualConfig
	.Create(DefaultConfig.Instance)
	.WithOptions(ConfigOptions.JoinSummary)
	.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared))
	.WithOptions(ConfigOptions.DisableLogFile)
	.HideColumns(Column.Job, Column.StdDev, Column.Median)
;

BenchmarkRunner.Run(
	typeof(Program).Assembly,
	benchmarkConfig
);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Benchmark methods can not be static
#pragma warning disable CA1050 // Declare types in namespaces
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable NetUlid.ToString() is not marked as pure function.

[MemoryDiagnoser]
public class Generate
{
	private static readonly NUlid.Rng.IUlidRng _nUlidRandomProvider = new NUlid.Rng.MonotonicUlidRng();
	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsR1Bp = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte,
		IncrementRandomSource = new ByteAether.Ulid.PseudoRandomProvider()
	};
	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsR4Bp = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte,
		IncrementRandomSource = new ByteAether.Ulid.PseudoRandomProvider()
	};
	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsR1Bc = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom1Byte,
		IncrementRandomSource = new ByteAether.Ulid.CryptographicallySecureRandomProvider()
	};
	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsR4Bc = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.MonotonicRandom4Byte,
		IncrementRandomSource = new ByteAether.Ulid.CryptographicallySecureRandomProvider()
	};

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New();

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlidR1Bp() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsR1Bp);

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlidR4Bp() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsR4Bp);

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlidR1Bc() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsR1Bc);

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlidR4Bc() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsR4Bc);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => global::NetUlid.Ulid.Generate();

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.NewUlid(_nUlidRandomProvider);
}

[MemoryDiagnoser]
public class GenerateNonMono
{
	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsNonMono = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic
	};

	private static readonly ByteAether.Ulid.Ulid.GenerationOptions _byteAetherUlidOptionsNonMonoP = new()
	{
		Monotonicity = ByteAether.Ulid.Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic,
		InitialRandomSource = new ByteAether.Ulid.PseudoRandomProvider()
	};

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsNonMono);

	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlidP() => ByteAether.Ulid.Ulid.New(_byteAetherUlidOptionsNonMonoP);

	[Benchmark]
	public System.Ulid Ulid() => System.Ulid.NewUlid();

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.NewUlid();

	[Benchmark]
	public System.Guid Guid() => System.Guid.NewGuid();

	[Benchmark]
	public System.Guid GuidV7() => System.Guid.CreateVersion7();
}

[MemoryDiagnoser]
public class ToByteArray : BenchmarkBase
{
	[Benchmark]
	public byte[] ByteAetherUlid() => _byteAether0[GetNextIndex()].ToByteArray();

	[Benchmark]
	public System.ReadOnlySpan<byte> AsByteSpan() => _byteAether0[GetNextIndex()].AsByteSpan();

	[Benchmark]
	public byte[] NetUlid() => _netUlid0[GetNextIndex()].ToByteArray();

	[Benchmark]
	public byte[] Ulid() => _cysharp0[GetNextIndex()].ToByteArray();

	[Benchmark]
	public byte[] NUlid() => _nulid0[GetNextIndex()].ToByteArray();
}

[MemoryDiagnoser]
public class FromByteArray : BenchmarkBase
{
	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(_bytes[GetNextIndex()]);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => new(_bytes[GetNextIndex()]);

	[Benchmark]
	public System.Ulid Ulid() => new(_bytes[GetNextIndex()]);

	[Benchmark]
	public NUlid.Ulid NUlid() => new(_bytes[GetNextIndex()]);

	[Benchmark]
	public System.Guid Guid() => new(_bytes[GetNextIndex()]);
}

[MemoryDiagnoser]
public class ToString : BenchmarkBase
{
	[Benchmark]
	public string ByteAetherUlid() => _byteAether0[GetNextIndex()].ToString();

	[Benchmark]
	public string NetUlid() => _netUlid0[GetNextIndex()].ToString();

	[Benchmark]
	public string Ulid() => _cysharp0[GetNextIndex()].ToString();

	[Benchmark]
	public string NUlid() => _nulid0[GetNextIndex()].ToString();

	[Benchmark]
	public string Guid() => _guid0[GetNextIndex()].ToString();
}

[MemoryDiagnoser]
public class FromString : BenchmarkBase
{
	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.Parse(_base32[GetNextIndex()]);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => global::NetUlid.Ulid.Parse(_base32[GetNextIndex()]);

	[Benchmark]
	public System.Ulid Ulid() => System.Ulid.Parse(_base32[GetNextIndex()]);

	[Benchmark]
	public NUlid.Ulid NUlid() => global::NUlid.Ulid.Parse(_base32[GetNextIndex()]);

	[Benchmark]
	public System.Guid Guid() => System.Guid.Parse(_guidHex[GetNextIndex()]);
}

[MemoryDiagnoser]
public class ToGuid : BenchmarkBase
{
	[Benchmark]
	public System.Guid ByteAetherUlid() => _byteAether0[GetNextIndex()].ToGuid();

	[Benchmark]
	public System.Guid NetUlid() => new(_netUlid0[GetNextIndex()].ToByteArray());

	[Benchmark]
	public System.Guid Ulid() => _cysharp0[GetNextIndex()].ToGuid();

	[Benchmark]
	public System.Guid NUlid() => _nulid0[GetNextIndex()].ToGuid();
}

[MemoryDiagnoser]
public class FromGuid : BenchmarkBase
{
	[Benchmark]
	public ByteAether.Ulid.Ulid ByteAetherUlid() => ByteAether.Ulid.Ulid.New(_guid0[GetNextIndex()]);

	[Benchmark]
	public NetUlid.Ulid NetUlid() => new(_guid0[GetNextIndex()].ToByteArray());

	[Benchmark]
	public System.Ulid Ulid() => new(_guid0[GetNextIndex()]);

	[Benchmark]
	public NUlid.Ulid NUlid() => new(_guid0[GetNextIndex()]);
}

[MemoryDiagnoser]
public class Equals : BenchmarkBase
{
	[Benchmark]
	public bool ByteAetherUlid()
	{
		var idx = GetNextIndex();
		return _byteAether0[idx].Equals(_byteAether1[idx]);
	}

	[Benchmark]
	public bool NetUlid()
	{
		var idx = GetNextIndex();
		return _netUlid0[idx].Equals(_netUlid1[idx]);
	}

	[Benchmark]
	public bool Ulid()
	{
		var idx = GetNextIndex();
		return _cysharp0[idx].Equals(_cysharp1[idx]);
	}

	[Benchmark]
	public bool NUlid()
	{
		var idx = GetNextIndex();
		return _nulid0[idx].Equals(_nulid1[idx]);
	}

	[Benchmark]
	public bool Guid()
	{
		var idx = GetNextIndex();
		return _guid0[idx].Equals(_guid1[idx]);
	}
}

[MemoryDiagnoser]
public class CompareTo : BenchmarkBase
{
	[Benchmark]
	public int ByteAetherUlid()
	{
		var idx = GetNextIndex();
		return _byteAether0[idx].CompareTo(_byteAether1[idx]);
	}

	[Benchmark]
	public int NetUlid()
	{
		var idx = GetNextIndex();
		return _netUlid0[idx].CompareTo(_netUlid1[idx]);
	}

	[Benchmark]
	public int Ulid()
	{
		var idx = GetNextIndex();
		return _cysharp0[idx].CompareTo(_cysharp1[idx]);
	}

	[Benchmark]
	public int NUlid()
	{
		var idx = GetNextIndex();
		return _nulid0[idx].CompareTo(_nulid1[idx]);
	}

	[Benchmark]
	public int Guid()
	{
		var idx = GetNextIndex();
		return _guid0[idx].CompareTo(_guid1[idx]);
	}
}

[MemoryDiagnoser]
public class GetHashCode : BenchmarkBase
{
	[Benchmark]
	public int ByteAetherUlid() => _byteAether0[GetNextIndex()].GetHashCode();

	[Benchmark]
	public int NetUlid() => _netUlid0[GetNextIndex()].GetHashCode();

	[Benchmark]
	public int Ulid() => _cysharp0[GetNextIndex()].GetHashCode();

	[Benchmark]
	public int NUlid() => _nulid0[GetNextIndex()].GetHashCode();

	[Benchmark]
	public int Guid() => _guid0[GetNextIndex()].GetHashCode();
}

public abstract class BenchmarkBase
{
	protected const int _iterationSize = 1024;
    private int _idx;

    protected ByteAether.Ulid.Ulid[] _byteAether0 = null!;
    protected ByteAether.Ulid.Ulid[] _byteAether1 = null!;

    protected NetUlid.Ulid[] _netUlid0 = null!;
    protected NetUlid.Ulid[] _netUlid1 = null!;

    protected System.Ulid[] _cysharp0 = null!;
    protected System.Ulid[] _cysharp1 = null!;

    protected NUlid.Ulid[] _nulid0 = null!;
    protected NUlid.Ulid[] _nulid1 = null!;

    protected System.Guid[] _guid0 = null!;
    protected System.Guid[] _guid1 = null!;

    protected string[] _base32 = null!;
    protected string[] _guidHex = null!;
    protected byte[][] _bytes = null!;

    /// <summary>
    /// Fetches the current index and increments/wraps it without branching.
    /// Inlining guarantees zero benchmark method call overhead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int GetNextIndex()
    {
	    var current = _idx;
	    _idx = (current + 1) & (_idx - 1);
	    return current;
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
	    _idx = 0;

        _byteAether0 = new ByteAether.Ulid.Ulid[_iterationSize];
        _byteAether1 = new ByteAether.Ulid.Ulid[_iterationSize];

        _netUlid0 = new NetUlid.Ulid[_iterationSize];
        _netUlid1 = new NetUlid.Ulid[_iterationSize];

        _cysharp0 = new System.Ulid[_iterationSize];
        _cysharp1 = new System.Ulid[_iterationSize];

        _nulid0 = new NUlid.Ulid[_iterationSize];
        _nulid1 = new NUlid.Ulid[_iterationSize];

        _guid0 = new System.Guid[_iterationSize];
        _guid1 = new System.Guid[_iterationSize];

        _base32 = new string[_iterationSize];
        _guidHex = new string[_iterationSize];
        _bytes = new byte[_iterationSize][];

        var rand = new System.Random(42);
        var leftBuffer = new byte[16];
        var rightBuffer = new byte[16];

        for (var i = 0; i < _iterationSize; i++)
        {
            rand.NextBytes(leftBuffer);
            rand.NextBytes(rightBuffer);

            // Scenarios
            switch (i % 4)
            {
	            case 0: break; // Left and Right are completely random
	            case 1: rightBuffer = leftBuffer; break; // Same values
	            case 2: System.Array.Copy(leftBuffer, 0, rightBuffer, 0, 6); break; // The first 6 bytes are the same (same timestamp)
	            case 3: System.Array.Copy(leftBuffer, 0, rightBuffer, 0, 15); break; // The first 15 bytes are the same (+1 increment)
            }

            _byteAether0[i] = ByteAether.Ulid.Ulid.New(leftBuffer);
            _byteAether1[i] = ByteAether.Ulid.Ulid.New(rightBuffer);

            _netUlid0[i] = new(leftBuffer);
            _netUlid1[i] = new(rightBuffer);

            _cysharp0[i] = new(leftBuffer);
            _cysharp1[i] = new(rightBuffer);

            _nulid0[i] = new(leftBuffer);
            _nulid1[i] = new(rightBuffer);

            _guid0[i] = new(leftBuffer);
            _guid1[i] = new(rightBuffer);

            _base32[i] = _byteAether0[i].ToString();
            _guidHex[i] = _guid0[i].ToString();
            _bytes[i] = leftBuffer;
        }
    }
}