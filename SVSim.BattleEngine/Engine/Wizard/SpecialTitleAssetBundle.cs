using System.IO;
using Cute;
using UnityEngine;

namespace Wizard;

public class SpecialTitleAssetBundle : MonoBehaviour
{

	private static string GetSpecialTitlePath(string id, bool isFetch)
	{
		return Toolbox.ResourcesManager.GetAssetTypePath(id, ResourcesManager.AssetLoadPathType.SpecialTitle, isFetch);
	}

	private static string GetBGMCueName(string id)
	{
		return "bgm_title_" + id;
	}

	private static bool IsBGMAvailable(string id)
	{
		return File.Exists(Path.Combine(Application.persistentDataPath, "b", GetBGMCueName(id) + ".awb"));
	}

	public static bool IsAvailableTitleAssetBundle(string id)
	{
		if (!IsBGMAvailable(id))
		{
			return false;
		}
		return File.Exists(new AssetHandle(GetSpecialTitlePath(id, isFetch: false), null, null, null, null, null).BuildLocalCachePath());
	}
}
