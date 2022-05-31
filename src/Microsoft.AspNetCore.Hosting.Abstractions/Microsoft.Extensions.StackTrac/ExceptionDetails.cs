using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class ExceptionDetails
{
	public Exception Error { get; set; }

	public IEnumerable<StackFrameSourceCodeInfo> StackFrames { get; set; }

	public string ErrorMessage { get; set; }
}
