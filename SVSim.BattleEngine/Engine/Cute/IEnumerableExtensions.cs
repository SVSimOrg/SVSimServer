using System.Collections.Generic;
using System.Linq;

namespace Cute;

public static class IEnumerableExtensions
{
	public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
	{
		return enumerable?.Any() ?? false;
	}
}
