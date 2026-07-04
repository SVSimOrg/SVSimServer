using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessRemoveAfterAction : SkillPreprocessBase
{
	public int Count { get; private set; } = 1;

	public SkillPreprocessRemoveAfterAction(string args, SkillBase skill)
	{
		if (args.Length < 2 || args.First() != '(' || args.Last() != ')')
		{
			return;
		}
		args = args.Substring(1, args.Length - 2);
		string[] array = args.Split(':');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			switch (array2[0])
			{
			case "skill_id":
				base.BanId = array2[1];
				break;
			case "count":
				Count = int.Parse(array2[1]);
				break;
			case "is_individual":
				base.BanId = (long.Parse(base.BanId) + skill.IndividualId).ToString();
				break;
			}
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		SetUp(skill);
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		SetUp(skill);
	}

	private void SetUp(SkillBase skill)
	{
		BattleCardBase card = skill.SkillPrm.ownerCard;
		List<SkillBase> sameBanIdSkills = new List<SkillBase>();
		if (base.BanId != string.Empty)
		{
			sameBanIdSkills = card.NormalSkills.Where((SkillBase s) => skill != s && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction && p.BanId == base.BanId)).ToList();
			sameBanIdSkills.AddRange(card.EvolutionSkills.Where((SkillBase s) => skill != s && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction && p.BanId == base.BanId)).ToList());
			IEnumerable<SkillPreprocessBase> source = sameBanIdSkills.SelectMany((SkillBase s) => s.PreprocessList.Where((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction && p.BanId == base.BanId));
			for (int num = 0; num < source.Count(); num++)
			{
				(source.ElementAt(num) as SkillPreprocessRemoveAfterAction).DecrementCount();
			}
		}
		SkillBase skillBase = card.NormalSkills.FirstOrDefault((SkillBase s) => skill != s && s.SkillPrm.buildInfo._previousSkillOwner != null && skill.SkillPrm.buildInfo == s.SkillPrm.buildInfo);
		if (skillBase != null)
		{
			sameBanIdSkills.Add(skillBase);
		}
		SkillBase skillBase2 = card.EvolutionSkills.FirstOrDefault((SkillBase s) => skill != s && s.SkillPrm.buildInfo._previousSkillOwner != null && skill.SkillPrm.buildInfo == s.SkillPrm.buildInfo);
		if (skillBase2 != null)
		{
			sameBanIdSkills.Add(skillBase2);
		}
		Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillBase _skill, List<BattleCardBase> cards, SkillConditionCheckerOption _checkerOption, SkillProcessor skillProcessorOneTime)
		{
			DecrementCount();
			skill.OnSkillEnd -= callStopOneTime;
			if (IsEnd())
			{
				card.NormalSkills.Remove(skill);
				card.EvolutionSkills.Remove(skill);
				for (int i = 0; i < sameBanIdSkills.Count(); i++)
				{
					SkillBase removeSkill = sameBanIdSkills.ElementAt(i);
					card.NormalSkills.Remove(removeSkill);
					card.EvolutionSkills.Remove(removeSkill);
					if (removeSkill.GetAttachSkill != null && !(card is IVirtualBattleCard))
					{
						StopSkill(removeSkill.GetAttachSkill, skillProcessorOneTime);
					}
					card.RemoveBuffInfo((BuffInfo b) => b.IsCopied && b.IsBuffGaveSkill(removeSkill));
				}
				SkillBase originSkill = skill.GetAttachSkill;
				if (originSkill != null && !(card is IVirtualBattleCard) && !card.Skills.Any((SkillBase s) => s.GetAttachSkill != null && s.GetAttachSkill.IsSameSkill(originSkill)))
				{
					StopSkill(skill.GetAttachSkill, skillProcessorOneTime);
				}
				card.RemoveBuffInfo((BuffInfo b) => b.IsCopied && b.IsBuffGaveSkill(skill));
			}
			return card.BattleCardView.InitializeBattleCardIcon(card, card.Skills);
		};
		skill.OnSkillEnd += callStopOneTime;
	}

	public void DecrementCount()
	{
		Count--;
	}

	public bool IsEnd()
	{
		return Count <= 0;
	}
}
