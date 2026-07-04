using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTimesPerTurn : SkillPreprocessTimesPerBase
{

	private BattleCardBase _owner;

	public bool IsSameBaseCardId { get; private set; }

	public SkillPreprocessTimesPerTurn(BattleCardBase card, string option)
		: base(int.Parse(option.Split(':')[0]))
	{
		_owner = card;
		string[] array = option.Split(':');
		IsSameBaseCardId = array.Any((string o) => o == "same_base_card_id");
		if (!IsSameBaseCardId && array.Count() > 1)
		{
			base.BanId = array[1];
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (!base.IsRight(playerInfoPair, option, PreexecutionCheck))
		{
			return false;
		}
		if (base.BanId != string.Empty)
		{
			return GetSkillNumInvokeCount(_owner, base.BanId) < _limitCount;
		}
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		base.Start(playerPair, skill, skillProcessor, optionValue, checkerOption);
		if (_invokeCount == 1)
		{
			BattlePlayerBase onTurnPlayer = (playerPair.Self.IsSelfTurn ? playerPair.Self : playerPair.Opponent);
			Func<SkillProcessor, VfxBase> callStopOneTime = null;
			callStopOneTime = delegate
			{
				_invokeCount = 0;
				onTurnPlayer.OnTurnEnd -= callStopOneTime;
				return NullVfx.GetInstance();
			};
			onTurnPlayer.OnTurnEnd += callStopOneTime;
		}
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		base.Clone(source, skill);
		if (source is SkillPreprocessTimesPerTurn skillPreprocessTimesPerTurn)
		{
			_owner = skillPreprocessTimesPerTurn._owner;
			base.BanId = skillPreprocessTimesPerTurn.BanId;
		}
	}

	public int GetInvokeCount()
	{
		return _invokeCount;
	}

	public void ResetInvokeCount()
	{
		_invokeCount = 0;
	}

	private int GetSkillNumInvokeCount(BattleCardBase card, string banId)
	{
		return GetSkillNumInvokeCount(card.NormalSkills, banId) + GetSkillNumInvokeCount(card.EvolutionSkills, banId);
	}

	private int GetSkillNumInvokeCount(SkillCollectionBase skills, string banId)
	{
		int num = 0;
		for (int i = 0; i < skills.Count(); i++)
		{
			List<SkillPreprocessBase> preprocessList = skills.ElementAt(i).PreprocessList;
			for (int j = 0; j < preprocessList.Count(); j++)
			{
				if (preprocessList[j] is SkillPreprocessTimesPerTurn && preprocessList[j].BanId == banId)
				{
					num += (preprocessList[j] as SkillPreprocessTimesPerTurn).GetInvokeCount();
				}
			}
		}
		return num;
	}
}
