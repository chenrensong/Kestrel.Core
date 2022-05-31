using System;

namespace Microsoft.AspNetCore.Http;

public interface IMiddlewareFactory
{
	IMiddleware Create(Type middlewareType);

	void Release(IMiddleware middleware);
}
