using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class StackFrameSourceCodeInfo
{
	public string Function { get; set; }

	public string File { get; set; }

	public int Line { get; set; }

	public int PreContextLine { get; set; }

	public IEnumerable<string> PreContextCode { get; set; } = Enumerable.Empty<string>();


	public IEnumerable<string> ContextCode { get; set; } = Enumerable.Empty<string>();


	public IEnumerable<string> PostContextCode { get; set; } = Enumerable.Empty<string>();


	public string ErrorDetails { get; set; }
}
