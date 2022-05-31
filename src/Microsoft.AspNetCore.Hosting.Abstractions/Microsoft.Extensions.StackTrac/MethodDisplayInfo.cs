using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class MethodDisplayInfo
{
	public string DeclaringTypeName { get; set; }

	public string Name { get; set; }

	public string GenericArguments { get; set; }

	public string SubMethod { get; set; }

	public IEnumerable<ParameterDisplayInfo> Parameters { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(DeclaringTypeName))
		{
			stringBuilder.Append(DeclaringTypeName).Append(".");
		}
		stringBuilder.Append(Name);
		stringBuilder.Append(GenericArguments);
		stringBuilder.Append("(");
		stringBuilder.Append(string.Join(", ", Parameters.Select((ParameterDisplayInfo p) => p.ToString())));
		stringBuilder.Append(")");
		if (!string.IsNullOrEmpty(SubMethod))
		{
			stringBuilder.Append("+");
			stringBuilder.Append(SubMethod);
			stringBuilder.Append("()");
		}
		return stringBuilder.ToString();
	}
}
