using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.UI;

public interface IClassInfomationUI
{
	void ShowInfomation(bool playEffect = true);

	void NewReplayUpdateInfomation(NetworkBattleReceiver.ClassInfoUiInfo classInfo);

	void HideInfomation();

	void HideOtherInfomation();

	void HideAllInfomation();

	VfxBase LoadResources(Transform parent, bool isPlayer);

	void SetUpEvent(BattlePlayerBase player);

	void Recovery();

	GameObject GetInfomationUI();

	void SetIsSelect(bool flg);

	void SetInCardFocus(bool flg);

	void SetTouchable(bool flg);
}
