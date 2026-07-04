using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SpellSkillCollection : SkillCollectionBase
{
	public SpellSkillCollection(BattleCardBase ownerCard)
		: base(ownerCard)
	{
	}

	public override VfxWith<SkillProcessor.ProcessInfo> CreateWhenPlayInfo(BattleCardBase playCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		SkillProcessor.ProcessInfo value = CreateProcessInfo((SkillBase s) => s.OnWhenPlayStart, skillProcessor, playerInfoPair, option);
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public override bool CheckWhenPlayCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, bool isPrePlay)
	{
		List<SkillBase> list = _skillList.Where((SkillBase s) => s.IsWhenPlaySkill).ToList();
		SkillBase item = list.First((SkillBase s) => s is Skill_spell_charge);
		list.Remove(item);
		return list.Any((SkillBase s) => s.CheckCondition(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay));
	}
}
