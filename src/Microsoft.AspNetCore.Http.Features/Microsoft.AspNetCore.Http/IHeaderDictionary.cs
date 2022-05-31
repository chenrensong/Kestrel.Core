using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public interface IHeaderDictionary : IDictionary<string, StringValues>, ICollection<KeyValuePair<string, StringValues>>, IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
{
	new StringValues this[string key] { get; set; }

	long? ContentLength { get; set; }
}
