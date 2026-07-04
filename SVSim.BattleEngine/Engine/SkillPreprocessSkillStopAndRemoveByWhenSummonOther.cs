using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSkillStopAndRemoveByWhenSummonOther : SkillPreprocessBase
{
	private readonly string _value;

	private readonly string _key;

	public SkillPreprocessSkillStopAndRemoveByWhenSummonOther(string info)
	{
		if (info.Length < 2 || info.First() != '(' || info.Last() != ')')
		{
			return;
		}
		info = info.Substring(1, info.Length - 2);
		string[] array = info.Split(':');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			string text = array2[0];
			if (text != null && text == "me_summoned_card_unit_clan")
			{
				_key = array2[0];
				_value = array2[1];
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
		Func<SkillProcessor, List<BattleCardBase>, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessor, List<BattleCardBase> summonedList)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (_key == "me_summoned_card_unit_clan" && _value == SkillFilterCreator.ContentKeyword.all.ToStringCustom())
			{
				for (int i = 0; i < summonedList.Count; i++)
				{
					if (summonedList[i] != skill.SkillPrm.ownerCard && summonedList[i].IsPlayer == skill.SkillPrm.ownerCard.IsPlayer && summonedList[i].Clan == CardBasePrm.ClanType.ALL && summonedList[i].IsUnit)
					{
						skill.SkillPrm.ownerCard.SelfBattlePlayer.OnAfterSummonCardEvent -= callStopOneTime;
						skill.SkillPrm.ownerCard.OpponentBattlePlayer.OnAfterSummonCardEvent -= callStopOneTime;
						sequentialVfxPlayer.Register(StopSkill(skill, skillProcessor));
						skill.SkillPrm.ownerCard.Skills.Remove(skill);
					}
				}
			}
			sequentialVfxPlayer.Register(skill.SkillPrm.ownerCard.BattleCardView.InitializeBattleCardIcon(skill.SkillPrm.ownerCard, skill.SkillPrm.ownerCard.Skills));
			return sequentialVfxPlayer;
		};
		if (_key == "me_summoned_card_unit_clan" && _value == SkillFilterCreator.ContentKeyword.all.ToStringCustom())
		{
			skill.SkillPrm.ownerCard.SelfBattlePlayer.OnAfterSummonCardEvent += callStopOneTime;
			skill.SkillPrm.ownerCard.OpponentBattlePlayer.OnAfterSummonCardEvent += callStopOneTime;
		}
	}
}
