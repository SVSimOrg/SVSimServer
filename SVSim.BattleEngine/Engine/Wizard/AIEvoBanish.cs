using System.Collections.Generic;

namespace Wizard;

public class AIEvoBanish : AIEvoTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIEvoBanish(string text)
		: base(text)
	{
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBanishSimulationUtility.BanishAll(targetsFromField, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AIBanishSimulationUtility.BanishRandom(targetsFromField, tagOwner, field, playPtn, situation);
				break;
			}
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AIBanishSimulationUtility.ExecuteTargetSelectBanish(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType, selectCount);
				break;
			}
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (candidate.IsIndependent || candidate.IsUnbanishable)
		{
			return false;
		}
		return IsCertainlyIncludeTarget(owner, candidate, situation);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Banish;
	}
}
