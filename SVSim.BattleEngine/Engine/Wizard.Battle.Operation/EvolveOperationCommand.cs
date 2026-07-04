using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Operation;

public class EvolveOperationCommand : SkillOperationCommandBase
{
	public EvolveOperationCommand(JsonData actionJsonData)
		: base(actionJsonData)
	{
	}

	public override void Operation(BattleManagerBase battleMgr)
	{
		BattleCardBase battleCardBase = battleMgr.GetBattlePlayer(_cardInfo.IsPlayer).InPlayCards.SingleOrDefault((BattleCardBase c) => c.Index == _cardInfo.Index);
		if (battleCardBase == null)
		{
			throw new Exception("場に " + _cardInfo.Name + " が見つかりませんでした");
		}
		SetupSkillSummon();
		VfxWith<List<BattleCardBase>> skillSelectedCardsWithVfx = GetSkillSelectedCardsWithVfx(battleCardBase, isEvolution: true);
		VfxBase vfx = battleMgr.OperateMgr.EvolutionCard(battleCardBase, _cardInfo.IsPlayer, skillSelectedCardsWithVfx.Value);
		battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
	}

	public override string ToString()
	{
		return "Operation evolve " + _cardInfo.Name;
	}
}
