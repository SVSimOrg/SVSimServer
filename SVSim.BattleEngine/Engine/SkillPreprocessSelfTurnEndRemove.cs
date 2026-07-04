using System;
using System.Linq;
using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessSelfTurnEndRemove : SkillPreprocessBase
{
	private int _turnEndCount = 1;

	private Func<SkillProcessor, VfxBase> _removeTurnEndCount;

	public SkillPreprocessSelfTurnEndRemove(SkillBase skill, string args)
	{
		SkillPreprocessSelfTurnEndRemove skillPreprocessSelfTurnEndRemove = this;
		BattleCardBase ownerCard = skill.SkillPrm.ownerCard;
		BattlePlayerBase battlePlayer = ownerCard.SelfBattlePlayer;
		if (args.Length >= 2 && args.First() == '(' && args.Last() == ')')
		{
			args = args.Substring(1, args.Length - 2);
			string[] array = args.Split('=');
			if (array[0] == "count")
			{
				_turnEndCount = int.Parse(array[1]);
			}
		}
		_removeTurnEndCount = delegate(SkillProcessor _)
		{
			skillPreprocessSelfTurnEndRemove._turnEndCount--;
			if (skillPreprocessSelfTurnEndRemove.IsEnd())
			{
				ownerCard.Skills.Remove(skill);
				if (skill.GetAttachSkill != null && !(ownerCard is IVirtualBattleCard))
				{
					skillPreprocessSelfTurnEndRemove.StopSkill(skill.GetAttachSkill, _);
				}
				battlePlayer.OnTurnEnd -= skillPreprocessSelfTurnEndRemove._removeTurnEndCount;
			}
			return NullVfx.GetInstance();
		};
		battlePlayer.OnTurnEnd += _removeTurnEndCount;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}

	private bool IsEnd()
	{
		return _turnEndCount <= 0;
	}
}
