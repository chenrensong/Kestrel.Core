using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.WebUtilities;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.AspNetCore.WebUtilities.Resources", typeof(Resources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string HttpRequestStreamReader_StreamNotReadable => ResourceManager.GetString("HttpRequestStreamReader_StreamNotReadable", resourceCulture);

	internal static string HttpResponseStreamWriter_StreamNotWritable => ResourceManager.GetString("HttpResponseStreamWriter_StreamNotWritable", resourceCulture);

	internal static string WebEncoders_InvalidCountOffsetOrLength => ResourceManager.GetString("WebEncoders_InvalidCountOffsetOrLength", resourceCulture);

	internal Resources()
	{
	}
}
