using System.Diagnostics;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class StackFrameInfo
{
	public int LineNumber { get; set; }

	public string FilePath { get; set; }

	public StackFrame StackFrame { get; set; }

	public MethodDisplayInfo MethodDisplayInfo { get; set; }
}
