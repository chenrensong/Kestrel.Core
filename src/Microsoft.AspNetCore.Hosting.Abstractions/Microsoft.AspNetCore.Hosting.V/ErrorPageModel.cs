using System.Collections.Generic;
using Microsoft.Extensions.StackTrace.Sources;

namespace Microsoft.AspNetCore.Hosting.Views;

internal class ErrorPageModel
{
	public IEnumerable<ExceptionDetails> ErrorDetails { get; set; }

	public string RuntimeDisplayName { get; set; }

	public string RuntimeArchitecture { get; set; }

	public string ClrVersion { get; set; }

	public string CurrentAssemblyVesion { get; set; }

	public string OperatingSystemDescription { get; set; }
}
