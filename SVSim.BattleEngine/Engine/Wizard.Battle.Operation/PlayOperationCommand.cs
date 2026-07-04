using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using Wizard.Battle.Recovery;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Operation;

public class PlayOperationCommand : SkillOperationCommandBase
{
	public PlayOperationCommand(JsonData actionJsonData)
		: base(actionJsonData)
	{
	}

	public override void Operation(BattleManagerBase battleMgr)
	{
		BattlePlayerBase battlePlayer = battleMgr.GetBattlePlayer(_cardInfo.IsPlayer);
		BattleCardBase playCard = battlePlayer.HandCardList.SingleOrDefault((BattleCardBase c) => c.Index == _cardInfo.Index);
		bool isChoiceBrave = false;
		if (playCard == null && _cardInfo.Index == 0)
		{
			playCard = battlePlayer.Class;
			isChoiceBrave = true;
		}
		if (playCard == null)
		{
			if (!(UIManager.GetInstance().NowOpenDialog != null))
			{
				UnityEngine.Debug.LogError("手札に " + _cardInfo.Name + " が見つかりませんでした");
				RecoveryManagerBase.OpenRecoveryFailedDialog();
			}
			return;
		}
		SetupSkillSummon();
		VfxWith<List<BattleCardBase>> skillSelectedCardsWithVfx = GetSkillSelectedCardsWithVfx(playCard, isEvolution: false, (bool isTargetSelectSkill) => battleMgr.OperateMgr.InitSetCard(playCard, _cardInfo.IsPlayer, isTargetSelectSkill, isRecovery: true));
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(skillSelectedCardsWithVfx.Vfx);
		sequentialVfxPlayer.Register(battleMgr.OperateMgr.PlayCard(playCard, _cardInfo.IsPlayer, skillSelectedCardsWithVfx.Value, isRecovery: true, null, isChoiceBrave));
		battleMgr.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
	}

	public override string ToString()
	{
		return "Operation play " + _cardInfo.Name;
	}
}
