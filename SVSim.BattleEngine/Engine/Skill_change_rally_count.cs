using System.Linq;
using Wizard.Battle.View.Vfx;

public class Skill_change_rally_count : SkillBase
{
	public Skill_change_rally_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int addCount = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_rally_count, 0);
		BattleCardBase[] array = parameter.targetCards.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SelfBattlePlayer.AddRallyCount(addCount);
		}
		return NullVfxWithLoading.GetInstance();
	}
}
