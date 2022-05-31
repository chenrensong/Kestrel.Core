using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http;

public interface IRequestCookieCollection : IEnumerable<KeyValuePair<string, string>>, IEnumerable
{
	int Count { get; }

	ICollection<string> Keys { get; }

	string this[string key] { get; }

	bool ContainsKey(string key);

	bool TryGetValue(string key, out string value);
}
