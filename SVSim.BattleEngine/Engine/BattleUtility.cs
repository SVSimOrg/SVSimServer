using System.Collections.Generic;
using System.Linq;

public static class BattleUtility
{
	public class RepeatSkillsAndTiming
	{
		public List<SkillBase> _skills;

		public List<string> _timings;

		public RepeatSkillsAndTiming(List<SkillBase> skills, List<string> timings)
		{
			_timings = timings;
			_skills = skills;
		}
	}

	public static RepeatSkillsAndTiming GetRepeatSkillsWithTiming(List<SkillBase> skills)
	{
		if (skills == null || skills.Count == 0)
		{
			return null;
		}
		BattleCardBase ownerCard = skills.First().SkillPrm.ownerCard;
		return GetRepeatSkillsWithTiming(ownerCard.SelfBattlePlayer, ownerCard, skills);
	}

	public static List<SkillBase> GetRepeatSkillsForPrediction(BattlePlayerBase sourcePlayer, BattleCardBase virtualCard)
	{
		if (virtualCard.Skills == null || virtualCard.Skills.Count() == 0)
		{
			return null;
		}
		return GetRepeatSkillsWithTiming(sourcePlayer, virtualCard, virtualCard.Skills.ToList())?._skills;
	}

	public static List<SkillBase> GetRepeatableWhenPlaySkill(BattleCardBase ownerCard, List<SkillBase> activeSkills, bool isInvokeCheck)
	{
		if (ownerCard.Skills.Any((SkillBase s) => s.IsUserSelectType && !(s is Skill_fusion)))
		{
			return null;
		}
		if (isInvokeCheck && !ownerCard.HasSkillWhenPlay(isOnlyNoSelect: true))
		{
			return null;
		}
		if (activeSkills.Count == 1 && activeSkills.Any((SkillBase s) => s is Skill_pp_fixeduse))
		{
			return null;
		}
		return activeSkills.Where((SkillBase c) => c.OnWhenPlayStart != 0).ToList();
	}

	private static RepeatSkillsAndTiming GetRepeatSkillsWithTiming(BattlePlayerBase player, BattleCardBase ownerCard, List<SkillBase> activeSkills)
	{
		List<SkillBase> list = new List<SkillBase>();
		List<string> list2 = new List<string>();
		IEnumerable<SkillBase> enumerable = activeSkills.Where((SkillBase s) => s.IsWhenDestroySkill);
		if (player.Class.SkillApplyInformation.RepeatSkillTimingList.Any((RepeatSkillInfo s) => s.Timing == "when_destroy" && Skill_repeat_skill.CheckCardType(s.Target, ownerCard)) && enumerable.Count() > 0)
		{
			list.AddRange(enumerable);
			list2.Add("when_destroy");
		}
		return new RepeatSkillsAndTiming(list, list2);
	}
}
