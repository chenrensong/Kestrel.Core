using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public abstract class HttpResponse
{
	private static readonly Func<object, Task> _callbackDelegate = (object callback) => ((Func<Task>)callback)();

	private static readonly Func<object, Task> _disposeDelegate = delegate(object disposable)
	{
		((IDisposable)disposable).Dispose();
		return Task.CompletedTask;
	};

	public abstract HttpContext HttpContext { get; }

	public abstract int StatusCode { get; set; }

	public abstract IHeaderDictionary Headers { get; }

	public abstract Stream Body { get; set; }

	public abstract long? ContentLength { get; set; }

	public abstract string ContentType { get; set; }

	public abstract IResponseCookies Cookies { get; }

	public abstract bool HasStarted { get; }

	public abstract void OnStarting(Func<object, Task> callback, object state);

	public virtual void OnStarting(Func<Task> callback)
	{
		OnStarting(_callbackDelegate, callback);
	}

	public abstract void OnCompleted(Func<object, Task> callback, object state);

	public virtual void RegisterForDispose(IDisposable disposable)
	{
		OnCompleted(_disposeDelegate, disposable);
	}

	public virtual void OnCompleted(Func<Task> callback)
	{
		OnCompleted(_callbackDelegate, callback);
	}

	public virtual void Redirect(string location)
	{
		Redirect(location, permanent: false);
	}

	public abstract void Redirect(string location, bool permanent);
}
