using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public interface IFormCollection : IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
{
	int Count { get; }

	ICollection<string> Keys { get; }

	StringValues this[string key] { get; }

	IFormFileCollection Files { get; }

	bool ContainsKey(string key);

	bool TryGetValue(string key, out StringValues value);
}
