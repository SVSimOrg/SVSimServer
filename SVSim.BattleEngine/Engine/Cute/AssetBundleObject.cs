using System.Collections.Generic;
using UnityEngine;

namespace Cute;

public class AssetBundleObject
{
	public AssetBundle assetBundle { get; set; }

	public List<AssetObject> objectArray { get; set; }

	public AssetBundleObject()
	{
		assetBundle = null;
		objectArray = new List<AssetObject>();
	}
}
