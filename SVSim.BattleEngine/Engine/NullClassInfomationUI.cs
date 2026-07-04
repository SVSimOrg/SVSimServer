using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class NullClassInfomationUI : IClassInfomationUI
{
	private static NullClassInfomationUI _instance;

	public static NullClassInfomationUI GetInstance()
	{
		if (_instance == null)
		{
			_instance = new NullClassInfomationUI();
		}
		return _instance;
	}

	protected NullClassInfomationUI()
	{
	}

	public void ShowInfomation(bool playEffect)
	{
	}

	public void NewReplayUpdateInfomation(NetworkBattleReceiver.ClassInfoUiInfo classInfo)
	{
	}

	public void HideInfomation()
	{
	}

	public void HideOtherInfomation()
	{
	}

	public void HideAllInfomation()
	{
	}

	public VfxBase LoadResources(Transform parent, bool isPlayer)
	{
		return NullVfx.GetInstance();
	}

	public void SetUpEvent(BattlePlayerBase player)
	{
	}

	public void Recovery()
	{
	}

	public GameObject GetInfomationUI()
	{
		return null;
	}

	public void SetIsSelect(bool flg)
	{
	}

	public void SetInCardFocus(bool flg)
	{
	}

	public void SetTouchable(bool flag)
	{
	}
}
