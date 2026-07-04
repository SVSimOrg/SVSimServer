using Convention;
using Cute;
using UnityEngine;
using Wizard.RoomMatch;

namespace Wizard;

public static class SoftwareReset
{

	private static string _bootScene;

	private static void resetAction()
	{
		Data.Clear();
		GameObject gameObject = GameObject.Find("_GameMgr");
		if (gameObject != null)
		{
			Object.Destroy(gameObject);
		}
		GameObject gameObject2 = GameObject.Find("_Game");
		if (gameObject2 != null)
		{
			Object.Destroy(gameObject2);
		}
		BGMManager.Dispose();
		Global.GAME_FONT = null;
		Global.IS_LOAD_ALLDONE = false;
	}

	public static void setAction()
	{
		SoftwareResetBase.setSoftwareResetAction(resetAction);
	}

	public static void exec(string sceneName = null, bool isFromUserDelete = false)
	{
		UIManager.GetInstance().isBattleRecovery = false;
		_bootScene = sceneName;

		UIManager.GetInstance().CreatFadeClose();
		VideoHostingUtil.OnSoftwareReset();
		RoomBase.OnSoftwareReset();
		Offline.OnSoftwareReset();
		SealedController.OnSoftwareReset();
		PlayerPrefsCache.OnSoftwareReset();
		// Pre-Phase-5b: released the battle mgr's UI GameObjects via BattleCtrl.BattleRelease
		// + BattleManagerBase.DisposeBattleGameObj. BattleControl is a stub (chunk 7) and headless
		// has nothing to dispose; both branches are unreachable safely.
	}
}
