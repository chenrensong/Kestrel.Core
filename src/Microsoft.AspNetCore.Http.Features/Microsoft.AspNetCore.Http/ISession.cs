using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public interface ISession
{
	bool IsAvailable { get; }

	string Id { get; }

	IEnumerable<string> Keys { get; }

	Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken));

	Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

	bool TryGetValue(string key, out byte[] value);

	void Set(string key, byte[] value);

	void Remove(string key);

	void Clear();
}
