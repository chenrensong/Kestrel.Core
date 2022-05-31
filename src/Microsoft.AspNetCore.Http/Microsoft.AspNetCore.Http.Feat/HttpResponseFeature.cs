using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public class HttpResponseFeature : IHttpResponseFeature
{
	public int StatusCode { get; set; }

	public string ReasonPhrase { get; set; }

	public IHeaderDictionary Headers { get; set; }

	public Stream Body { get; set; }

	public virtual bool HasStarted => false;

	public HttpResponseFeature()
	{
		StatusCode = 200;
		Headers = new HeaderDictionary();
		Body = Stream.Null;
	}

	public virtual void OnStarting(Func<object, Task> callback, object state)
	{
	}

	public virtual void OnCompleted(Func<object, Task> callback, object state)
	{
	}
}
