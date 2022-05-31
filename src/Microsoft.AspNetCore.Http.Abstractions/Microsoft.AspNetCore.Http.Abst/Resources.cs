using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.AspNetCore.Http.Abstractions;

internal static class Resources
{
	private static readonly ResourceManager _resourceManager = new ResourceManager("Microsoft.AspNetCore.Http.Abstractions.Resources", typeof(Resources).GetTypeInfo().Assembly);

	internal static string Exception_UseMiddlewareIServiceProviderNotAvailable => GetString("Exception_UseMiddlewareIServiceProviderNotAvailable");

	internal static string Exception_UseMiddlewareNoInvokeMethod => GetString("Exception_UseMiddlewareNoInvokeMethod");

	internal static string Exception_UseMiddlewareNonTaskReturnType => GetString("Exception_UseMiddlewareNonTaskReturnType");

	internal static string Exception_UseMiddlewareNoParameters => GetString("Exception_UseMiddlewareNoParameters");

	internal static string Exception_UseMiddleMutlipleInvokes => GetString("Exception_UseMiddleMutlipleInvokes");

	internal static string Exception_PathMustStartWithSlash => GetString("Exception_PathMustStartWithSlash");

	internal static string Exception_InvokeMiddlewareNoService => GetString("Exception_InvokeMiddlewareNoService");

	internal static string Exception_InvokeDoesNotSupportRefOrOutParams => GetString("Exception_InvokeDoesNotSupportRefOrOutParams");

	internal static string Exception_PortMustBeGreaterThanZero => GetString("Exception_PortMustBeGreaterThanZero");

	internal static string Exception_UseMiddlewareNoMiddlewareFactory => GetString("Exception_UseMiddlewareNoMiddlewareFactory");

	internal static string Exception_UseMiddlewareUnableToCreateMiddleware => GetString("Exception_UseMiddlewareUnableToCreateMiddleware");

	internal static string Exception_UseMiddlewareExplicitArgumentsNotSupported => GetString("Exception_UseMiddlewareExplicitArgumentsNotSupported");

	internal static string ArgumentCannotBeNullOrEmpty => GetString("ArgumentCannotBeNullOrEmpty");

	internal static string FormatException_UseMiddlewareIServiceProviderNotAvailable(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareIServiceProviderNotAvailable"), p0);
	}

	internal static string FormatException_UseMiddlewareNoInvokeMethod(object p0, object p1, object p2)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareNoInvokeMethod"), p0, p1, p2);
	}

	internal static string FormatException_UseMiddlewareNonTaskReturnType(object p0, object p1, object p2)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareNonTaskReturnType"), p0, p1, p2);
	}

	internal static string FormatException_UseMiddlewareNoParameters(object p0, object p1, object p2)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareNoParameters"), p0, p1, p2);
	}

	internal static string FormatException_UseMiddleMutlipleInvokes(object p0, object p1)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddleMutlipleInvokes"), p0, p1);
	}

	internal static string FormatException_PathMustStartWithSlash(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_PathMustStartWithSlash"), p0);
	}

	internal static string FormatException_InvokeMiddlewareNoService(object p0, object p1)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_InvokeMiddlewareNoService"), p0, p1);
	}

	internal static string FormatException_InvokeDoesNotSupportRefOrOutParams(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_InvokeDoesNotSupportRefOrOutParams"), p0);
	}

	internal static string FormatException_PortMustBeGreaterThanZero()
	{
		return GetString("Exception_PortMustBeGreaterThanZero");
	}

	internal static string FormatException_UseMiddlewareNoMiddlewareFactory(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareNoMiddlewareFactory"), p0);
	}

	internal static string FormatException_UseMiddlewareUnableToCreateMiddleware(object p0, object p1)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareUnableToCreateMiddleware"), p0, p1);
	}

	internal static string FormatException_UseMiddlewareExplicitArgumentsNotSupported(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, GetString("Exception_UseMiddlewareExplicitArgumentsNotSupported"), p0);
	}

	internal static string FormatArgumentCannotBeNullOrEmpty()
	{
		return GetString("ArgumentCannotBeNullOrEmpty");
	}

	private static string GetString(string name, params string[] formatterNames)
	{
		string text = _resourceManager.GetString(name);
		if (formatterNames != null)
		{
			for (int i = 0; i < formatterNames.Length; i++)
			{
				text = text.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
			}
		}
		return text;
	}
}
