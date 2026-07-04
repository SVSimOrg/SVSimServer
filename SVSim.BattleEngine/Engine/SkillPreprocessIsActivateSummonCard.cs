using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessIsActivateSummonCard : SkillPreprocessBase
{
	private readonly bool _flag;

	private SkillBase _skill;

	public SkillPreprocessIsActivateSummonCard(SkillBase skill, string flag)
	{
		_skill = skill;
		_flag = flag == "true";
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int num = 5 - playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Count();
		bool flag = !PreexecutionCheck || num > 0;
		List<Skill_cant_summon.CantSummonInfo> cantSummonList = _skill.SkillPrm.ownerCard.SelfBattlePlayer.Class.SkillApplyInformation.CantSummonList;
		for (int i = 0; i < cantSummonList.Count; i++)
		{
			if (cantSummonList[i] == Skill_cant_summon.CantSummonInfo.DeckSelf)
			{
				flag = false;
			}
		}
		return flag == _flag;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
