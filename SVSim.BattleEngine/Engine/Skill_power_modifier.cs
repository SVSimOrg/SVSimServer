using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_power_modifier : Skill_powerup
{
	public override bool IsTargetIndicate => false;

	public Skill_power_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override void RegisterWhenBuffInfo(BattleCardBase card, CallParameter parameter)
	{
		if (_addOffense > 0 || _addLife > 0)
		{
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(card.SelfBattlePlayer, card.OpponentBattlePlayer);
			parameter.skillProcessor.Register(card.Skills.CreateWhenBuffInfo(parameter.skillProcessor, playerInfoPair, card));
		}
	}

	protected override VfxBase DeadCheck(BattleCardBase targetCard, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (targetCard.IsDead)
		{
			sequentialVfxPlayer.Register(targetCard.SelfBattlePlayer.CardManagement(targetCard, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this));
		}
		return sequentialVfxPlayer;
	}

	protected override void AddBattleLog(IEnumerable<BattleCardBase> targetCards)
	{
		base.AddBattleLog(targetCards);
		List<BattleCardBase> list = targetCards.Where((BattleCardBase c) => c.IsLifeZeroDead).ToList();
		if (list.Count > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillBuffAdd(list, -_gainOffense, -_gainLife, this, isMinusZeroAttack: false, isMinusZeroLife: false);
		}
	}
}
