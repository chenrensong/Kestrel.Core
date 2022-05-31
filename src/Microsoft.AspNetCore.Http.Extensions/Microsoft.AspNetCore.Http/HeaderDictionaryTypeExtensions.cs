using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

public static class HeaderDictionaryTypeExtensions
{
	private static IDictionary<Type, object> KnownParsers = new Dictionary<Type, object>
	{
		{
			typeof(CacheControlHeaderValue),
			(Func<string, CacheControlHeaderValue>)((string value) => (!CacheControlHeaderValue.TryParse(value, out var parsedValue7)) ? null : parsedValue7)
		},
		{
			typeof(ContentDispositionHeaderValue),
			(Func<string, ContentDispositionHeaderValue>)((string value) => (!ContentDispositionHeaderValue.TryParse(value, out var parsedValue6)) ? null : parsedValue6)
		},
		{
			typeof(ContentRangeHeaderValue),
			(Func<string, ContentRangeHeaderValue>)((string value) => (!ContentRangeHeaderValue.TryParse(value, out var parsedValue5)) ? null : parsedValue5)
		},
		{
			typeof(MediaTypeHeaderValue),
			(Func<string, MediaTypeHeaderValue>)((string value) => (!MediaTypeHeaderValue.TryParse(value, out var parsedValue4)) ? null : parsedValue4)
		},
		{
			typeof(RangeConditionHeaderValue),
			(Func<string, RangeConditionHeaderValue>)((string value) => (!RangeConditionHeaderValue.TryParse(value, out var parsedValue3)) ? null : parsedValue3)
		},
		{
			typeof(RangeHeaderValue),
			(Func<string, RangeHeaderValue>)((string value) => (!RangeHeaderValue.TryParse(value, out var parsedValue2)) ? null : parsedValue2)
		},
		{
			typeof(EntityTagHeaderValue),
			(Func<string, EntityTagHeaderValue>)((string value) => (!EntityTagHeaderValue.TryParse(value, out var parsedValue)) ? null : parsedValue)
		},
		{
			typeof(DateTimeOffset?),
			(Func<string, DateTimeOffset?>)((string value) => (!HeaderUtilities.TryParseDate(value, out var result2)) ? null : new DateTimeOffset?(result2))
		},
		{
			typeof(long?),
			(Func<string, long?>)((string value) => (!HeaderUtilities.TryParseNonNegativeInt64(value, out var result)) ? null : new long?(result))
		}
	};

	private static IDictionary<Type, object> KnownListParsers = new Dictionary<Type, object>
	{
		{
			typeof(MediaTypeHeaderValue),
			(Func<IList<string>, IList<MediaTypeHeaderValue>>)((IList<string> value) => (!MediaTypeHeaderValue.TryParseList(value, out var parsedValues5)) ? null : parsedValues5)
		},
		{
			typeof(StringWithQualityHeaderValue),
			(Func<IList<string>, IList<StringWithQualityHeaderValue>>)((IList<string> value) => (!StringWithQualityHeaderValue.TryParseList(value, out var parsedValues4)) ? null : parsedValues4)
		},
		{
			typeof(CookieHeaderValue),
			(Func<IList<string>, IList<CookieHeaderValue>>)((IList<string> value) => (!CookieHeaderValue.TryParseList(value, out var parsedValues3)) ? null : parsedValues3)
		},
		{
			typeof(EntityTagHeaderValue),
			(Func<IList<string>, IList<EntityTagHeaderValue>>)((IList<string> value) => (!EntityTagHeaderValue.TryParseList(value, out var parsedValues2)) ? null : parsedValues2)
		},
		{
			typeof(SetCookieHeaderValue),
			(Func<IList<string>, IList<SetCookieHeaderValue>>)((IList<string> value) => (!SetCookieHeaderValue.TryParseList(value, out var parsedValues)) ? null : parsedValues)
		}
	};

	public static RequestHeaders GetTypedHeaders(this HttpRequest request)
	{
		return new RequestHeaders(request.Headers);
	}

	public static ResponseHeaders GetTypedHeaders(this HttpResponse response)
	{
		return new ResponseHeaders(response.Headers);
	}

	internal static DateTimeOffset? GetDate(this IHeaderDictionary headers, string name)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return headers.Get<DateTimeOffset?>(name);
	}

	internal static void Set(this IHeaderDictionary headers, string name, object value)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			headers.Remove(name);
		}
		else
		{
			headers[name] = value.ToString();
		}
	}

	internal static void SetList<T>(this IHeaderDictionary headers, string name, IList<T> values)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (values == null || values.Count == 0)
		{
			headers.Remove(name);
			return;
		}
		if (values.Count == 1)
		{
			headers[name] = new StringValues(values[0].ToString());
			return;
		}
		string[] array = new string[values.Count];
		for (int i = 0; i < values.Count; i++)
		{
			array[i] = values[i].ToString();
		}
		headers[name] = new StringValues(array);
	}

	public static void AppendList<T>(this IHeaderDictionary Headers, string name, IList<T> values)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		switch (values.Count)
		{
		case 0:
			Headers.Append(name, StringValues.Empty);
			return;
		case 1:
			Headers.Append(name, new StringValues(values[0].ToString()));
			return;
		}
		string[] array = new string[values.Count];
		for (int i = 0; i < values.Count; i++)
		{
			array[i] = values[i].ToString();
		}
		Headers.Append(name, new StringValues(array));
	}

	internal static void SetDate(this IHeaderDictionary headers, string name, DateTimeOffset? value)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value.HasValue)
		{
			headers[name] = HeaderUtilities.FormatDate(value.Value);
		}
		else
		{
			headers.Remove(name);
		}
	}

	internal static T Get<T>(this IHeaderDictionary headers, string name)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		StringValues stringValues = headers[name];
		if (StringValues.IsNullOrEmpty(stringValues))
		{
			return default(T);
		}
		if (KnownParsers.TryGetValue(typeof(T), out var value))
		{
			return ((Func<string, T>)value)(stringValues);
		}
		return GetViaReflection<T>(stringValues.ToString());
	}

	internal static IList<T> GetList<T>(this IHeaderDictionary headers, string name)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		StringValues stringValues = headers[name];
		if (StringValues.IsNullOrEmpty(stringValues))
		{
			return null;
		}
		if (KnownListParsers.TryGetValue(typeof(T), out var value))
		{
			return ((Func<IList<string>, IList<T>>)value)(stringValues);
		}
		return GetListViaReflection<T>(stringValues);
	}

	private static T GetViaReflection<T>(string value)
	{
		Type type = typeof(T);
		MethodInfo methodInfo2 = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo methodInfo)
		{
			if (string.Equals("TryParse", methodInfo.Name, StringComparison.Ordinal) && methodInfo.ReturnParameter.ParameterType.Equals(typeof(bool)))
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(string)) && parameters[1].IsOut)
				{
					return parameters[1].ParameterType.Equals(type.MakeByRefType());
				}
				return false;
			}
			return false;
		});
		if (methodInfo2 == null)
		{
			throw new NotSupportedException(string.Format("The given type '{0}' does not have a TryParse method with the required signature 'public static bool TryParse(string, out {0}).", "T"));
		}
		object[] array = new object[2] { value, null };
		if ((bool)methodInfo2.Invoke(null, array))
		{
			return (T)array[1];
		}
		return default(T);
	}

	private static IList<T> GetListViaReflection<T>(StringValues values)
	{
		MethodInfo methodInfo2 = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo methodInfo)
		{
			if (string.Equals("TryParseList", methodInfo.Name, StringComparison.Ordinal) && methodInfo.ReturnParameter.ParameterType.Equals(typeof(bool)))
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(IList<string>)) && parameters[1].IsOut)
				{
					return parameters[1].ParameterType.Equals(typeof(IList<T>).MakeByRefType());
				}
				return false;
			}
			return false;
		});
		if (methodInfo2 == null)
		{
			throw new NotSupportedException(string.Format("The given type '{0}' does not have a TryParseList method with the required signature 'public static bool TryParseList(IList<string>, out IList<{0}>).", "T"));
		}
		object[] array = new object[2] { values, null };
		if ((bool)methodInfo2.Invoke(null, array))
		{
			return (IList<T>)array[1];
		}
		return null;
	}
}
