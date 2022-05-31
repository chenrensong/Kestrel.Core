using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Http.Extensions;

public class QueryBuilder : IEnumerable<KeyValuePair<string, string>>, IEnumerable
{
	private IList<KeyValuePair<string, string>> _params;

	public QueryBuilder()
	{
		_params = new List<KeyValuePair<string, string>>();
	}

	public QueryBuilder(IEnumerable<KeyValuePair<string, string>> parameters)
	{
		_params = new List<KeyValuePair<string, string>>(parameters);
	}

	public void Add(string key, IEnumerable<string> values)
	{
		foreach (string value in values)
		{
			_params.Add(new KeyValuePair<string, string>(key, value));
		}
	}

	public void Add(string key, string value)
	{
		_params.Add(new KeyValuePair<string, string>(key, value));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		for (int i = 0; i < _params.Count; i++)
		{
			KeyValuePair<string, string> keyValuePair = _params[i];
			stringBuilder.Append(flag ? "?" : "&");
			flag = false;
			stringBuilder.Append(UrlEncoder.Default.Encode(keyValuePair.Key));
			stringBuilder.Append("=");
			stringBuilder.Append(UrlEncoder.Default.Encode(keyValuePair.Value));
		}
		return stringBuilder.ToString();
	}

	public QueryString ToQueryString()
	{
		return new QueryString(ToString());
	}

	public override int GetHashCode()
	{
		return ToQueryString().GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return ToQueryString().Equals(obj);
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return _params.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _params.GetEnumerator();
	}
}
