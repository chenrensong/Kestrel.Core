using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public interface IQueryCollection : IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
{
	int Count { get; }

	ICollection<string> Keys { get; }

	StringValues this[string key] { get; }

	bool ContainsKey(string key);

	bool TryGetValue(string key, out StringValues value);
}
