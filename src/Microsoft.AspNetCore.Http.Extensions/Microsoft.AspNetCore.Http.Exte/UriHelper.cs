using System;
using System.Text;

namespace Microsoft.AspNetCore.Http.Extensions;

public static class UriHelper
{
	private const string ForwardSlash = "/";

	private const string Pound = "#";

	private const string QuestionMark = "?";

	private const string SchemeDelimiter = "://";

	public static string BuildRelative(PathString pathBase = default(PathString), PathString path = default(PathString), QueryString query = default(QueryString), FragmentString fragment = default(FragmentString))
	{
		return ((pathBase.HasValue || path.HasValue) ? (pathBase + path).ToString() : "/") + query.ToString() + fragment.ToString();
	}

	public static string BuildAbsolute(string scheme, HostString host, PathString pathBase = default(PathString), PathString path = default(PathString), QueryString query = default(QueryString), FragmentString fragment = default(FragmentString))
	{
		if (scheme == null)
		{
			throw new ArgumentNullException("scheme");
		}
		string text = ((pathBase.HasValue || path.HasValue) ? (pathBase + path).ToString() : "/");
		string text2 = host.ToString();
		string text3 = query.ToString();
		string text4 = fragment.ToString();
		return new StringBuilder(scheme.Length + "://".Length + text2.Length + text.Length + text3.Length + text4.Length).Append(scheme).Append("://").Append(text2)
			.Append(text)
			.Append(text3)
			.Append(text4)
			.ToString();
	}

	public static void FromAbsolute(string uri, out string scheme, out HostString host, out PathString path, out QueryString query, out FragmentString fragment)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		path = default(PathString);
		query = default(QueryString);
		fragment = default(FragmentString);
		int num = uri.IndexOf("://");
		if (num < 0)
		{
			throw new FormatException("No scheme delimiter in uri.");
		}
		scheme = uri.Substring(0, num);
		num += "://".Length;
		int num2 = -1;
		int num3 = uri.Length;
		if ((num2 = uri.IndexOf("#", num)) >= 0 && num2 < num3)
		{
			fragment = FragmentString.FromUriComponent(uri.Substring(num2));
			num3 = num2;
		}
		if ((num2 = uri.IndexOf("?", num)) >= 0 && num2 < num3)
		{
			query = QueryString.FromUriComponent(uri.Substring(num2, num3 - num2));
			num3 = num2;
		}
		if ((num2 = uri.IndexOf("/", num)) >= 0 && num2 < num3)
		{
			path = PathString.FromUriComponent(uri.Substring(num2, num3 - num2));
			num3 = num2;
		}
		host = HostString.FromUriComponent(uri.Substring(num, num3 - num));
	}

	public static string Encode(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (uri.IsAbsoluteUri)
		{
			return BuildAbsolute(uri.Scheme, HostString.FromUriComponent(uri), PathString.FromUriComponent(uri), default(PathString), QueryString.FromUriComponent(uri), FragmentString.FromUriComponent(uri));
		}
		return uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
	}

	public static string GetEncodedUrl(this HttpRequest request)
	{
		return BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path, request.QueryString);
	}

	public static string GetEncodedPathAndQuery(this HttpRequest request)
	{
		return BuildRelative(request.PathBase, request.Path, request.QueryString);
	}

	public static string GetDisplayUrl(this HttpRequest request)
	{
		string value = request.Host.Value;
		string value2 = request.PathBase.Value;
		string value3 = request.Path.Value;
		string value4 = request.QueryString.Value;
		return new StringBuilder(request.Scheme.Length + "://".Length + value.Length + value2.Length + value3.Length + value4.Length).Append(request.Scheme).Append("://").Append(value)
			.Append(value2)
			.Append(value3)
			.Append(value4)
			.ToString();
	}
}
