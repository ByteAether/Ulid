namespace ByteAether.Ulid;

/// <summary>
/// Implementations of this interface dictate the method by which randomness
/// is produced.
/// </summary>
public interface IRandomProvider
{
	/// <summary>
	/// Fills the specified buffer with random byte values.<br/>
	/// The implementation determines the source of randomness.
	/// </summary>
	/// <param name="buffer">
	/// The span of bytes to be filled with random data. The length of
	/// the buffer determines how many bytes are generated.
	/// </param>
	public void GetBytes(Span<byte> buffer);
}