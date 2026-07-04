using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BetterList<T>
{
	public delegate int CompareFunc(T left, T right);

	public T[] buffer;

	public int size;

	[DebuggerHidden]
	public T this[int i]
	{
		get
		{
			return buffer[i];
		}
		set
		{
			buffer[i] = value;
		}
	}

	private void AllocateMore()
	{
		T[] array = ((buffer != null) ? new T[Mathf.Max(buffer.Length << 1, 32)] : new T[32]);
		if (buffer != null && size > 0)
		{
			buffer.CopyTo(array, 0);
		}
		buffer = array;
	}

	public void Clear()
	{
		size = 0;
	}

	public void Add(T item)
	{
		if (buffer == null || size == buffer.Length)
		{
			AllocateMore();
		}
		buffer[size++] = item;
	}

	public bool Remove(T item)
	{
		if (buffer != null)
		{
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < size; i++)
			{
				if (equalityComparer.Equals(buffer[i], item))
				{
					size--;
					buffer[i] = default(T);
					for (int j = i; j < size; j++)
					{
						buffer[j] = buffer[j + 1];
					}
					buffer[size] = default(T);
					return true;
				}
			}
		}
		return false;
	}
}
