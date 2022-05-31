using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public static class HttpResponseWritingExtensions
{
	public static Task WriteAsync(this HttpResponse response, string text, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		return response.WriteAsync(text, Encoding.UTF8, cancellationToken);
	}

	public static Task WriteAsync(this HttpResponse response, string text, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		byte[] bytes = encoding.GetBytes(text);
		return response.Body.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
	}
}
