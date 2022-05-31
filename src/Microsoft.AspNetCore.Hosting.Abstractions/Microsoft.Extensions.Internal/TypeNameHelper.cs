using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Internal;

internal class TypeNameHelper
{
	private struct DisplayNameOptions
	{
		public bool FullName { get; }

		public bool IncludeGenericParameterNames { get; }

		public DisplayNameOptions(bool fullName, bool includeGenericParameterNames)
		{
			FullName = fullName;
			IncludeGenericParameterNames = includeGenericParameterNames;
		}
	}

	private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
	{
		{
			typeof(void),
			"void"
		},
		{
			typeof(bool),
			"bool"
		},
		{
			typeof(byte),
			"byte"
		},
		{
			typeof(char),
			"char"
		},
		{
			typeof(decimal),
			"decimal"
		},
		{
			typeof(double),
			"double"
		},
		{
			typeof(float),
			"float"
		},
		{
			typeof(int),
			"int"
		},
		{
			typeof(long),
			"long"
		},
		{
			typeof(object),
			"object"
		},
		{
			typeof(sbyte),
			"sbyte"
		},
		{
			typeof(short),
			"short"
		},
		{
			typeof(string),
			"string"
		},
		{
			typeof(uint),
			"uint"
		},
		{
			typeof(ulong),
			"ulong"
		},
		{
			typeof(ushort),
			"ushort"
		}
	};

	public static string GetTypeDisplayName(object item, bool fullName = true)
	{
		if (item != null)
		{
			return GetTypeDisplayName(item.GetType(), fullName);
		}
		return null;
	}

	public static string GetTypeDisplayName(Type type, bool fullName = true, bool includeGenericParameterNames = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ProcessType(stringBuilder, type, new DisplayNameOptions(fullName, includeGenericParameterNames));
		return stringBuilder.ToString();
	}

	private static void ProcessType(StringBuilder builder, Type type, DisplayNameOptions options)
	{
		string value;
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			ProcessGenericType(builder, type, genericArguments, genericArguments.Length, options);
		}
		else if (type.IsArray)
		{
			ProcessArrayType(builder, type, options);
		}
		else if (_builtInTypeNames.TryGetValue(type, out value))
		{
			builder.Append(value);
		}
		else if (type.IsGenericParameter)
		{
			if (options.IncludeGenericParameterNames)
			{
				builder.Append(type.Name);
			}
		}
		else
		{
			builder.Append(options.FullName ? type.FullName : type.Name);
		}
	}

	private static void ProcessArrayType(StringBuilder builder, Type type, DisplayNameOptions options)
	{
		Type type2 = type;
		while (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		ProcessType(builder, type2, options);
		while (type.IsArray)
		{
			builder.Append('[');
			builder.Append(',', type.GetArrayRank() - 1);
			builder.Append(']');
			type = type.GetElementType();
		}
	}

	private static void ProcessGenericType(StringBuilder builder, Type type, Type[] genericArguments, int length, DisplayNameOptions options)
	{
		int num = 0;
		if (type.IsNested)
		{
			num = type.DeclaringType.GetGenericArguments().Length;
		}
		if (options.FullName)
		{
			if (type.IsNested)
			{
				ProcessGenericType(builder, type.DeclaringType, genericArguments, num, options);
				builder.Append('+');
			}
			else if (!string.IsNullOrEmpty(type.Namespace))
			{
				builder.Append(type.Namespace);
				builder.Append('.');
			}
		}
		int num2 = type.Name.IndexOf('`');
		if (num2 <= 0)
		{
			builder.Append(type.Name);
			return;
		}
		builder.Append(type.Name, 0, num2);
		builder.Append('<');
		for (int i = num; i < length; i++)
		{
			ProcessType(builder, genericArguments[i], options);
			if (i + 1 != length)
			{
				builder.Append(',');
				if (options.IncludeGenericParameterNames || !genericArguments[i + 1].IsGenericParameter)
				{
					builder.Append(' ');
				}
			}
		}
		builder.Append('>');
	}
}
