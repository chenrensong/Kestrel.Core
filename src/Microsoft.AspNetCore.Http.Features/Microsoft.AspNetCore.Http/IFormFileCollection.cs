using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http;

public interface IFormFileCollection : IReadOnlyList<IFormFile>, IEnumerable<IFormFile>, IEnumerable, IReadOnlyCollection<IFormFile>
{
	IFormFile this[string name] { get; }

	IFormFile GetFile(string name);

	IReadOnlyList<IFormFile> GetFiles(string name);
}
