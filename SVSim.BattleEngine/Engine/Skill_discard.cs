using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_discard : SkillBase
{
	public override bool ShowSideLog
	{
		get
		{
			if (base.ShowSideLog)
			{
				if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdmin && !base.SkillPrm.ownerCard.IsPlayer)
				{
					return !base.SkillPrm.ownerCard.IsInCemetery;
				}
				return true;
			}
			return false;
		}
	}

	public Skill_discard(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase c) => c.IsPlayer == base.SkillPrm.selfBattlePlayer.IsPlayer).ToList();
		List<BattleCardBase> list2 = parameter.targetCards.Where((BattleCardBase c) => c.IsPlayer == base.SkillPrm.opponentBattlePlayer.IsPlayer).ToList();
		base.SkillPrm.selfBattlePlayer.SkillDiscards.AddRange(list);
		base.SkillPrm.opponentBattlePlayer.SkillDiscards.AddRange(list2);
		base.SkillPrm.selfBattlePlayer.DiscardedCardList.AddRange(list);
		base.SkillPrm.selfBattlePlayer.FusionIngredientAndDiscardedCardList.AddRange(list);
		if (list.Count > 0)
		{
			BattlePlayerBase battlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
			TurnAndIntValue turnAndIntValue = battlePlayer.GameSkillDiscardCountList.FirstOrDefault((TurnAndIntValue t) => t.IsSelfTurn == battlePlayer.IsSelfTurn && t.Turn == battlePlayer.Turn);
			if (turnAndIntValue != null)
			{
				turnAndIntValue.Increment();
			}
			else
			{
				battlePlayer.GameSkillDiscardCountList.Add(new TurnAndIntValue(1, battlePlayer.Turn, battlePlayer.IsSelfTurn));
			}
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDiscard(list, base.SkillPrm.selfBattlePlayer.IsPlayer, this);
			BattleLogManager.GetInstance().AddLogSkillDiscard(list2, base.SkillPrm.opponentBattlePlayer.IsPlayer, this);
		}
		base.SkillPrm.selfBattlePlayer.SelfDiscardList.Add(list.SingleOrDefault((BattleCardBase s) => s == base.SkillPrm.ownerCard));
		return VfxWithLoading.Create(base.SkillPrm.selfBattlePlayer.CardManagement(list, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, this));
	}
}
