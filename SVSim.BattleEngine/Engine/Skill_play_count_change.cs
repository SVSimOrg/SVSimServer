using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_play_count_change : SkillBase
{
	public Skill_play_count_change(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int addCount = GetAddCount();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			BattlePlayerBase selfBattlePlayer = targetCard.SelfBattlePlayer;
			selfBattlePlayer.AddCurrentTrunPlayCount(addCount);
			if (selfBattlePlayer.ClassInformationUIController != null && IsBattleLog)
			{
				SkillBase getAttachSkill = base.GetAttachSkill;
				if (getAttachSkill != null && getAttachSkill.SkillPrm.ownerCard.IsSpecialSkill)
				{
					selfBattlePlayer.ClassInformationUIController.UpdateInfomation();
				}
				else
				{
					selfBattlePlayer.ClassInformationUIController.ShowInfomation();
				}
			}
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillChangePlayCount(base.SkillPrm.ownerCard.SelfBattlePlayer.Class, addCount, this);
		}
		return NullVfxWithLoading.GetInstance();
	}

	public int GetAddCount()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_count, 0);
	}
}
