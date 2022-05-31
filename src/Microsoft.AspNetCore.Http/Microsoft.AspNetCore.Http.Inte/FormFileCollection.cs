using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Internal;

public class FormFileCollection : List<IFormFile>, IFormFileCollection, IReadOnlyList<IFormFile>, IEnumerable<IFormFile>, IEnumerable, IReadOnlyCollection<IFormFile>
{
	public IFormFile this[string name] => GetFile(name);

	public IFormFile GetFile(string name)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				IFormFile current = enumerator.Current;
				if (string.Equals(name, current.Name, StringComparison.OrdinalIgnoreCase))
				{
					return current;
				}
			}
		}
		return null;
	}

	public IReadOnlyList<IFormFile> GetFiles(string name)
	{
		List<IFormFile> list = new List<IFormFile>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IFormFile current = enumerator.Current;
			if (string.Equals(name, current.Name, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(current);
			}
		}
		return list;
	}
}
