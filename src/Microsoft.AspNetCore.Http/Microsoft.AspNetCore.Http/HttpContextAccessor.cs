using System.Threading;

namespace Microsoft.AspNetCore.Http;

public class HttpContextAccessor : IHttpContextAccessor
{
	private class HttpContextHolder
	{
		public HttpContext Context;
	}

	private static AsyncLocal<HttpContextHolder> _httpContextCurrent = new AsyncLocal<HttpContextHolder>();

	public HttpContext HttpContext
	{
		get
		{
			return _httpContextCurrent.Value?.Context;
		}
		set
		{
			HttpContextHolder value2 = _httpContextCurrent.Value;
			if (value2 != null)
			{
				value2.Context = null;
			}
			if (value != null)
			{
				_httpContextCurrent.Value = new HttpContextHolder
				{
					Context = value
				};
			}
		}
	}
}
