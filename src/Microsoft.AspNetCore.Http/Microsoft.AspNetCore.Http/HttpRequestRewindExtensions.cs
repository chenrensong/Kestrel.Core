using Microsoft.AspNetCore.Http.Internal;

namespace Microsoft.AspNetCore.Http;

public static class HttpRequestRewindExtensions
{
	public static void EnableBuffering(this HttpRequest request)
	{
		request.EnableRewind();
	}

	public static void EnableBuffering(this HttpRequest request, int bufferThreshold)
	{
		request.EnableRewind(bufferThreshold);
	}

	public static void EnableBuffering(this HttpRequest request, long bufferLimit)
	{
		request.EnableRewind(30720, bufferLimit);
	}

	public static void EnableBuffering(this HttpRequest request, int bufferThreshold, long bufferLimit)
	{
		request.EnableRewind(bufferThreshold, bufferLimit);
	}
}
