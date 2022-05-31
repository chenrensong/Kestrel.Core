using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Internal;

public class FormFile : IFormFile
{
	private const int DefaultBufferSize = 81920;

	private readonly Stream _baseStream;

	private readonly long _baseStreamOffset;

	public string ContentDisposition
	{
		get
		{
			return Headers["Content-Disposition"];
		}
		set
		{
			Headers["Content-Disposition"] = value;
		}
	}

	public string ContentType
	{
		get
		{
			return Headers["Content-Type"];
		}
		set
		{
			Headers["Content-Type"] = value;
		}
	}

	public IHeaderDictionary Headers { get; set; }

	public long Length { get; }

	public string Name { get; }

	public string FileName { get; }

	public FormFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
	{
		_baseStream = baseStream;
		_baseStreamOffset = baseStreamOffset;
		Length = length;
		Name = name;
		FileName = fileName;
	}

	public Stream OpenReadStream()
	{
		return new ReferenceReadStream(_baseStream, _baseStreamOffset, Length);
	}

	public void CopyTo(Stream target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		using Stream stream = OpenReadStream();
		stream.CopyTo(target, 81920);
	}

	public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		using Stream readStream = OpenReadStream();
		await readStream.CopyToAsync(target, 81920, cancellationToken);
	}
}
