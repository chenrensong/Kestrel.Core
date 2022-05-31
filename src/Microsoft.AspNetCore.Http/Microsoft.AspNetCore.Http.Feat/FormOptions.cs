namespace Microsoft.AspNetCore.Http.Features;

public class FormOptions
{
	public const int DefaultMemoryBufferThreshold = 65536;

	public const int DefaultBufferBodyLengthLimit = 134217728;

	public const int DefaultMultipartBoundaryLengthLimit = 128;

	public const long DefaultMultipartBodyLengthLimit = 134217728L;

	public bool BufferBody { get; set; }

	public int MemoryBufferThreshold { get; set; } = 65536;


	public long BufferBodyLengthLimit { get; set; } = 134217728L;


	public int ValueCountLimit { get; set; } = 1024;


	public int KeyLengthLimit { get; set; } = 2048;


	public int ValueLengthLimit { get; set; } = 4194304;


	public int MultipartBoundaryLengthLimit { get; set; } = 128;


	public int MultipartHeadersCountLimit { get; set; } = 16;


	public int MultipartHeadersLengthLimit { get; set; } = 16384;


	public long MultipartBodyLengthLimit { get; set; } = 134217728L;

}
