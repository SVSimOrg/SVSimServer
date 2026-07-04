using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_pp_fixeduse : SkillBase
{
	public int _fixedUsePP;

	public override bool IsTargetIndicate => false;

	public bool IsMutationOnCost => base.SkillPrm.ownerCard.Cost > _fixedUsePP;

	public bool IsAccelerateOrCrystallize
	{
		get
		{
			if (OnWhenAccelerate == 0)
			{
				return OnWhenCrystallize != 0;
			}
			return true;
		}
	}

	public bool IsMutationFixedUseCost
	{
		get
		{
			if (base.SkillPrm.ownerCard.Cost > _fixedUsePP && base.SkillPrm.ownerCard.SelfBattlePlayer.Pp >= _fixedUsePP)
			{
				return base.SkillPrm.ownerCard.Cost > base.SkillPrm.ownerCard.SelfBattlePlayer.Pp;
			}
			return false;
		}
	}

	public Skill_pp_fixeduse(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		string s = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.fixeduse);
		_fixedUsePP = int.Parse(s);
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (IsBattleLog)
		{
			BattleCardBase ownerCard = base.SkillPrm.ownerCard;
			if (IsEnhance())
			{
				BattleLogManager.GetInstance().UpdateSkillTiming(ownerCard, LogType.Play, LogType.Enhance);
			}
		}
		return NullVfxWithLoading.GetInstance();
	}
}
