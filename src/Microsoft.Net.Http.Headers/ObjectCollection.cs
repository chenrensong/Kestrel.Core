using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Net.Http.Headers;

internal class ObjectCollection<T> : Collection<T>
{
	internal static readonly Action<T> DefaultValidator = CheckNotNull;

	internal static readonly ObjectCollection<T> EmptyReadOnlyCollection = new ObjectCollection<T>(DefaultValidator, isReadOnly: true);

	private readonly Action<T> _validator;

	public bool IsReadOnly => ((ICollection<T>)this).IsReadOnly;

	private static IList<T> CreateInnerList(bool isReadOnly, IEnumerable<T> other = null)
	{
		List<T> list = ((other == null) ? new List<T>() : new List<T>(other));
		if (isReadOnly)
		{
			return new ReadOnlyCollection<T>(list);
		}
		return list;
	}

	public ObjectCollection()
		: this(DefaultValidator, isReadOnly: false)
	{
	}

	public ObjectCollection(Action<T> validator, bool isReadOnly = false)
		: base(CreateInnerList(isReadOnly))
	{
		_validator = validator;
	}

	public ObjectCollection(IEnumerable<T> other, bool isReadOnly = false)
		: base(CreateInnerList(isReadOnly, other))
	{
		_validator = DefaultValidator;
		foreach (T item in base.Items)
		{
			_validator(item);
		}
	}

	protected override void ClearItems()
	{
		base.ClearItems();
	}

	protected override void InsertItem(int index, T item)
	{
		_validator(item);
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, T item)
	{
		_validator(item);
		base.SetItem(index, item);
	}

	private static void CheckNotNull(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
	}
}
