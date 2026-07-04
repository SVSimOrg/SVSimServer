using UnityEngine;

namespace Wizard;

public class UIDestroyUtility
{
	public static void RemoveChildren(Transform trans)
	{
		for (int num = trans.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(trans.GetChild(num).gameObject);
		}
	}
}
