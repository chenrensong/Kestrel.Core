using System.Reflection;
using System.Resources;

namespace Microsoft.AspNetCore.Hosting;

internal static class Resources
{
	private static readonly ResourceManager _resourceManager = new ResourceManager("Microsoft.AspNetCore.Hosting.Resources", typeof(Resources).GetTypeInfo().Assembly);

	internal static string ErrorPageHtml_Title => GetString("ErrorPageHtml_Title");

	internal static string ErrorPageHtml_UnhandledException => GetString("ErrorPageHtml_UnhandledException");

	internal static string ErrorPageHtml_UnknownLocation => GetString("ErrorPageHtml_UnknownLocation");

	internal static string WebHostBuilder_SingleInstance => GetString("WebHostBuilder_SingleInstance");

	internal static string FormatErrorPageHtml_Title()
	{
		return GetString("ErrorPageHtml_Title");
	}

	internal static string FormatErrorPageHtml_UnhandledException()
	{
		return GetString("ErrorPageHtml_UnhandledException");
	}

	internal static string FormatErrorPageHtml_UnknownLocation()
	{
		return GetString("ErrorPageHtml_UnknownLocation");
	}

	internal static string FormatWebHostBuilder_SingleInstance()
	{
		return GetString("WebHostBuilder_SingleInstance");
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
