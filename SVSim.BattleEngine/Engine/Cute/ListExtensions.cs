using System.Collections.Generic;
using UnityEngine;

namespace Cute;

public static class ListExtensions
{
	public static void FisherYatesShuffle<T>(this List<T> listToShuffle)
	{
		for (int num = listToShuffle.Count - 1; num > 0; num--)
		{
			int index = Random.Range(0, num + 1);
			T value = listToShuffle[index];
			listToShuffle[index] = listToShuffle[num];
			listToShuffle[num] = value;
		}
	}
}
