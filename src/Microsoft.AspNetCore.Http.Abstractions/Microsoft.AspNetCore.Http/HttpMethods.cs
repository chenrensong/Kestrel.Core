using System;

namespace Microsoft.AspNetCore.Http;

public static class HttpMethods
{
	public static readonly string Connect = "CONNECT";

	public static readonly string Delete = "DELETE";

	public static readonly string Get = "GET";

	public static readonly string Head = "HEAD";

	public static readonly string Options = "OPTIONS";

	public static readonly string Patch = "PATCH";

	public static readonly string Post = "POST";

	public static readonly string Put = "PUT";

	public static readonly string Trace = "TRACE";

	public static bool IsConnect(string method)
	{
		if ((object)Connect != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Connect, method);
		}
		return true;
	}

	public static bool IsDelete(string method)
	{
		if ((object)Delete != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Delete, method);
		}
		return true;
	}

	public static bool IsGet(string method)
	{
		if ((object)Get != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Get, method);
		}
		return true;
	}

	public static bool IsHead(string method)
	{
		if ((object)Head != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Head, method);
		}
		return true;
	}

	public static bool IsOptions(string method)
	{
		if ((object)Options != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Options, method);
		}
		return true;
	}

	public static bool IsPatch(string method)
	{
		if ((object)Patch != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Patch, method);
		}
		return true;
	}

	public static bool IsPost(string method)
	{
		if ((object)Post != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Post, method);
		}
		return true;
	}

	public static bool IsPut(string method)
	{
		if ((object)Put != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Put, method);
		}
		return true;
	}

	public static bool IsTrace(string method)
	{
		if ((object)Trace != method)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(Trace, method);
		}
		return true;
	}
}
