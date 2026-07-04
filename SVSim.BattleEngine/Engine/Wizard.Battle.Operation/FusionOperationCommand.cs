using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard.AutoTest;
using Wizard.Battle.Recovery;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Operation;

public class FusionOperationCommand : SkillOperationCommandBase
{
	public FusionOperationCommand(JsonData actionJsonData)
		: base(actionJsonData)
	{
	}

	protected virtual BattleCardBase ReplaceCardData(BattleManagerBase battleMgr, BattleCardBase fusionCard)
	{
		return fusionCard;
	}

	public override void Operation(BattleManagerBase battleMgr)
	{
		BattleCardBase battleCardBase = battleMgr.GetBattlePlayer(_cardInfo.IsPlayer).HandCardList.SingleOrDefault((BattleCardBase c) => c.Index == _cardInfo.Index);
		if (battleCardBase == null)
		{
			if (!(UIManager.GetInstance().NowOpenDialog != null))
			{
				LocalLog.AccumulateTraceLog("Card not found in hand :" + _cardInfo.Index + " " + _cardInfo.Name);
				RecoveryManagerBase.OpenRecoveryFailedDialog();
			}
			return;
		}
		battleCardBase = ReplaceCardData(battleMgr, battleCardBase);
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (SkillTargetInfo skillTargetInfo in _skillTargetInfoList)
		{
			AutoTestBattleMgr.CardInfo targetCardInfo = skillTargetInfo.TargetCard;
			BattleCardBase item = battleMgr.GetBattlePlayer(targetCardInfo.IsPlayer).AllCards.Single((BattleCardBase c) => c.Index == targetCardInfo.Index);
			list.Add(item);
		}
		VfxBase vfx = battleMgr.OperateMgr.FusionCard(battleCardBase, _cardInfo.IsPlayer, list);
		battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
	}

	public override string ToString()
	{
		return "Operation comp_fusion " + _cardInfo.Name;
	}
}
