using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

internal class Skill_invoke_skill : SkillBase
{
	public enum SkillTiming
	{
		when_play,
		when_destroy
	}

	private bool _isAllowDestroyTarget;

	public List<SkillBase> InsertSkillList { get; private set; }

	public List<SkillBase> NotInsertSkillList { get; private set; }

	public override bool IsAllowDestroyTarget => _isAllowDestroyTarget;

	public Skill_invoke_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		InsertSkillList = new List<SkillBase>();
		NotInsertSkillList = new List<SkillBase>();
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
		{
			_isAllowDestroyTarget = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.is_allow_destroy_target) == "true";
		}
	}

	public bool IsInvokableSkill(SkillBase skill, string timing)
	{
		bool num = !skill.IsEnhance();
		bool flag = false;
		if (timing == SkillTiming.when_play.ToString())
		{
			flag = skill.IsWhenPlaySkill;
		}
		else if (timing == SkillTiming.when_destroy.ToString())
		{
			flag = skill.IsWhenDestroySkill;
		}
		return num && !skill.IsBurialRite && flag;
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential result = VfxWithLoadingSequential.Create();
		string invokeType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invoke_type);
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.ownerCard.SelfBattlePlayer, base.SkillPrm.ownerCard.OpponentBattlePlayer);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.IsSkipPpCheck = true;
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			List<SkillBase> list = new List<SkillBase>();
			List<SkillBase> list2 = new List<SkillBase>();
			List<SkillBase> list3 = new List<SkillBase>();
			bool flag = false;
			if (invokeType == SkillTiming.when_play.ToString())
			{
				list = targetCard.NormalSkills.Where((SkillBase s) => IsInvokableSkill(s, invokeType)).ToList();
			}
			else if (invokeType == SkillFilterCreator.ContentKeyword.when_play_except_burial_rite.ToString())
			{
				for (int num = 0; num < targetCard.Skills.Count(); num++)
				{
					SkillBase skillBase = targetCard.Skills.ElementAt(num);
					if (!skillBase.IsWhenPlaySkill || skillBase.IsUserSelectType || !skillBase.Used)
					{
						continue;
					}
					for (int num2 = 0; num2 < skillBase.PreprocessList.Count; num2++)
					{
						if (skillBase.PreprocessList[num2] is SkillPreprocessBurialRite skillPreprocessBurialRite)
						{
							skillPreprocessBurialRite.SetInvoked();
						}
					}
					for (int num3 = 0; num3 < skillBase.ConditionFilterCollection.ConditionCheckerFilterList.Count; num3++)
					{
						if (skillBase.ConditionFilterCollection.ConditionCheckerFilterList[num3] is SkillConditionBurialRite skillConditionBurialRite)
						{
							skillConditionBurialRite.SetInvoked();
						}
					}
					list.Add(skillBase);
				}
			}
			else
			{
				list = targetCard.Skills.Where((SkillBase s) => IsInvokableSkill(s, invokeType)).ToList();
			}
			foreach (SkillBase item in list)
			{
				if (item.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessTimesPerTurn) is SkillPreprocessTimesPerTurn skillPreprocessTimesPerTurn)
				{
					skillPreprocessTimesPerTurn.ResetInvokeCount();
				}
				bool flag2 = item.CheckCondition(playerInfoPair, skillConditionCheckerOption, isPrePlay: false);
				if (item.IsCheckLastTarget())
				{
					flag2 = flag2 && flag;
				}
				else
				{
					flag = flag2;
				}
				if (flag2)
				{
					list2.Add(item);
				}
				else
				{
					list3.Add(item);
				}
			}
			if (invokeType == SkillTiming.when_play.ToString())
			{
				list2 = BattleUtility.GetRepeatableWhenPlaySkill(targetCard, list2, isInvokeCheck: true);
			}
			if (list2 != null)
			{
				InsertSkillList.AddRange(list2);
				foreach (SkillBase item2 in list2)
				{
					item2.SetInvoked(flag: true);
				}
			}
			NotInsertSkillList.AddRange(list3);
		}
		return result;
	}
}
