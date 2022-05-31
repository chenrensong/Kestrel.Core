using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class DescribeSchemesContext
{
	private List<IDictionary<string, object>> _results;

	public IEnumerable<IDictionary<string, object>> Results => _results;

	public DescribeSchemesContext()
	{
		_results = new List<IDictionary<string, object>>();
	}

	public void Accept(IDictionary<string, object> description)
	{
		_results.Add(description);
	}
}
