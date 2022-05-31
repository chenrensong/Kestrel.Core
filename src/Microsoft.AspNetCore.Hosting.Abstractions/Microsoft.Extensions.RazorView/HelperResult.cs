using System;
using System.IO;

namespace Microsoft.Extensions.RazorViews;

internal class HelperResult
{
	public Action<TextWriter> WriteAction { get; }

	public HelperResult(Action<TextWriter> action)
	{
		WriteAction = action;
	}

	public void WriteTo(TextWriter writer)
	{
		WriteAction(writer);
	}
}
