using System;
using System.Linq;
using LitJson;
using Wizard.AutoTest;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Operation;

public class AttackOperationCommand : SkillOperationCommandBase
{
	private readonly AutoTestBattleMgr.CardInfo _targetCardInfo;

	public AttackOperationCommand(JsonData actionJsonData)
		: base(actionJsonData)
	{
		_targetCardInfo = new AutoTestBattleMgr.CardInfo(actionJsonData["target"].ToString());
	}

	public override void Operation(BattleManagerBase battleMgr)
	{
		BattlePlayerPair battlePlayerPair = battleMgr.GetBattlePlayerPair(_cardInfo.IsPlayer);
		BattleCardBase battleCardBase = battlePlayerPair.Self.InPlayCards.SingleOrDefault((BattleCardBase c) => c.Index == _cardInfo.Index);
		BattleCardBase battleCardBase2 = battlePlayerPair.Opponent.ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c.Index == _targetCardInfo.Index);
		if (battleCardBase == null)
		{
			throw new Exception("場に " + _cardInfo.Name + " が見つかりませんでした");
		}
		if (battleCardBase2 == null)
		{
			throw new Exception("場に " + _targetCardInfo.Name + " が見つかりませんでした");
		}
		SetupSkillSummon();
		VfxBase vfx = battleMgr.OperateMgr.Attack(battleCardBase, battleCardBase2, _cardInfo.IsPlayer);
		battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
	}

	public override string ToString()
	{
		return "Operation attack " + _cardInfo.Name + " to " + _targetCardInfo.Name;
	}
}
