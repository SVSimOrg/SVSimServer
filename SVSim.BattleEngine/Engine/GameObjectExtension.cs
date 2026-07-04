using UnityEngine;

public static class GameObjectExtension
{
	public static void SetLayer(this GameObject gameObject, int layer, bool isSetChildren)
	{
		if (gameObject == null)
		{
			return;
		}
		gameObject.layer = layer;
		if (!isSetChildren)
		{
			return;
		}
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayer(layer, isSetChildren);
		}
	}
}
