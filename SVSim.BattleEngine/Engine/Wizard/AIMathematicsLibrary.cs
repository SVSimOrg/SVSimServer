using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public static class AIMathematicsLibrary
{
	public static Dictionary<int, List<int[]>> PermListsDictionary = new Dictionary<int, List<int[]>>();

	public static IEnumerable<T[]> EnumeratePermutations<T>(List<T> numbers)
	{
		return _GetPermutations(new List<T>(), numbers);
	}

	public static List<int[]> EnumerateIndexListPermutations(int count)
	{
		if (PermListsDictionary.ContainsKey(count))
		{
			return PermListsDictionary[count];
		}
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			list.Add(i);
		}
		List<int[]> list2 = EnumeratePermutations(list).ToList();
		PermListsDictionary.Add(count, list2);
		return list2;
	}

	private static IEnumerable<T[]> _GetPermutations<T>(IEnumerable<T> seq, IEnumerable<T> numbers)
	{
		if (numbers.Count() == 0)
		{
			yield return seq.ToArray();
			yield break;
		}
		foreach (T number in numbers)
		{
			IEnumerable<T[]> enumerable = _GetPermutations(seq.Concat(new T[1] { number }), numbers.Where((T x) => !x.Equals(number)));
			foreach (T[] item in enumerable)
			{
				yield return item.ToArray();
			}
		}
	}

	public static IEnumerable<T[]> EnumerateCombinations<T>(List<T> numbers, int selectCount)
	{
		if (numbers.Count == 0)
		{
			yield return numbers.ToArray();
		}
		bool[] appearanceMap = new bool[numbers.Count];
		for (int i = 0; i < appearanceMap.Length; i++)
		{
			if (i < selectCount)
			{
				appearanceMap[i] = true;
			}
			else
			{
				appearanceMap[i] = false;
			}
		}
		bool flag = true;
		while (flag)
		{
			yield return numbers.FindAll(delegate(T n)
			{
				int num6 = numbers.IndexOf(n);
				return appearanceMap[num6];
			}).ToArray();
			flag = false;
			for (int num = 0; num < appearanceMap.Length - 1; num++)
			{
				if (!appearanceMap[num] || appearanceMap[num + 1])
				{
					continue;
				}
				appearanceMap[num] = false;
				appearanceMap[num + 1] = true;
				int num2 = 0;
				for (int num3 = 0; num3 < num; num3++)
				{
					if (appearanceMap[num3])
					{
						num2++;
					}
				}
				if (num2 < num)
				{
					for (int num4 = 0; num4 < num2; num4++)
					{
						appearanceMap[num4] = true;
					}
					for (int num5 = num2; num5 < num; num5++)
					{
						appearanceMap[num5] = false;
					}
				}
				flag = true;
				break;
			}
		}
	}
}
