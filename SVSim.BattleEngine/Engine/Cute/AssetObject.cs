using UnityEngine;

namespace Cute;

public class AssetObject
{
	public string basePath { get; private set; }

	public Object baseObject { get; private set; }

	public AssetObject(string path, Object obj)
	{
		basePath = path;
		baseObject = obj;
	}
}
