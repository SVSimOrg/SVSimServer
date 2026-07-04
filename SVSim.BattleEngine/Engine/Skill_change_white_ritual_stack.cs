using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_change_white_ritual_stack : SkillBase
{
	public Skill_change_white_ritual_stack(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add, -1);
		BattleCardBase newestInplayStack = base.SkillPrm.selfBattlePlayer.InPlayCards.Where((BattleCardBase c) => c.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL) && c.HasSkillStackWhiteRitual).LastOrDefault();
		if (newestInplayStack != null)
		{
			newestInplayStack.SkillApplyInformation.GiveWhiteRitualCount(num);
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			CallOnChangeWhiteRitualStack(newestInplayStack, num);
			if (newestInplayStack.SkillApplyInformation.WhiteRitualCount <= 0)
			{
				CallOnSkillDestroy();
				newestInplayStack.FlagCardAsDestroyedBySkill();
				sequentialVfxPlayer.Register(newestInplayStack.SelfBattlePlayer.CardManagement(newestInplayStack, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false, null, null, this));
			}
			if (IsBattleLog)
			{
				if (base.SkillPrm.selfBattlePlayer.SkillInfoCemeterys.Any((IReadOnlyBattleCardInfo c) => c == newestInplayStack))
				{
					BattleLogManager.GetInstance().AddLogSkillDeath(new List<BattleCardBase> { newestInplayStack }, this);
				}
				else if (num >= 0)
				{
					BattleLogManager.GetInstance().AddLogGiveWhiteRitualStack(num, newestInplayStack, this);
				}
				else
				{
					BattleLogManager.GetInstance().AddLogDepriveWhiteRitualStack(num, newestInplayStack, this);
				}
			}
		}
		vfxWithLoadingSequential.RegisterToMainVfx(sequentialVfxPlayer);
		return vfxWithLoadingSequential;
	}

	protected virtual void CallOnChangeWhiteRitualStack(BattleCardBase target, int changeCount)
	{
	}

	protected virtual void CallOnSkillDestroy()
	{
	}
}
