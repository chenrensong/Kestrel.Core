using System.Text;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class ParameterDisplayInfo
{
	public string Name { get; set; }

	public string Type { get; set; }

	public string Prefix { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(Prefix))
		{
			stringBuilder.Append(Prefix).Append(" ");
		}
		stringBuilder.Append(Type);
		stringBuilder.Append(" ");
		stringBuilder.Append(Name);
		return stringBuilder.ToString();
	}
}
