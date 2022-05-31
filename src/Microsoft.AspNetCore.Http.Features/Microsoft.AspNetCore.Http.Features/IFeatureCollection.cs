using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Features;

public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>, IEnumerable
{
	bool IsReadOnly { get; }

	int Revision { get; }

	object this[Type key] { get; set; }

	TFeature Get<TFeature>();

	void Set<TFeature>(TFeature instance);
}
