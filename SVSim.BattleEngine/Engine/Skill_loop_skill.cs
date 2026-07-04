using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

internal class Skill_loop_skill : SkillBase
{
	public List<SkillBase> LoopSkillList { get; private set; }

	public Skill_loop_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		LoopSkillList = new List<SkillBase>();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential result = VfxWithLoadingSequential.Create();
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.ownerCard.SelfBattlePlayer, base.SkillPrm.ownerCard.OpponentBattlePlayer);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.IsSkipPpCheck = true;
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.loop_range_before_this_skill);
		int count = base.SkillPrm.ownerCard.Skills.IndexOf(this) - num;
		num++;
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			IEnumerable<SkillBase> source = parameter.targetCards.ElementAt(i).Skills.Skip(count).Take(num);
			bool flag = false;
			for (int j = 0; j < source.Count(); j++)
			{
				SkillBase skillBase = source.ElementAt(j);
				bool flag2 = skillBase.CheckCondition(playerInfoPair, skillConditionCheckerOption, isPrePlay: false);
				if (skillBase.IsCheckLastTarget())
				{
					flag2 = flag2 && flag;
				}
				else
				{
					flag = flag2;
				}
				if (flag2)
				{
					LoopSkillList.Add(skillBase);
				}
			}
		}
		return result;
	}
}
