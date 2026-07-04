using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayDestroy : AIWhenPlayTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayDestroy(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[5]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT,
			AIScriptTokenArgType.OLDEST_SELECT
		};
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				UpdateSituationRemovalType(situation);
				AISkillSimulationUtility.DestroyAll(targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation, selectCount);
				break;
			}
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AIDestroySimulationUtility.ExecuteTargetSelectDestroy(tagOwner, targetsFromField, field, playPtn, situation, base.SelectType, selectCount);
				break;
			}
			case AIScriptTokenArgType.OLDEST_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AISkillSimulationUtility.DestroyOldest(targetsFromField, field, situation, selectCount);
				break;
			}
			default:
				AIConsoleUtility.LogError("AIWhenPlayDestroy.Execute() Error!! SelectType=" + base.SelectType);
				break;
			}
		}
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			TargetLifePredictionForAllSelect(target, owner, field, playPtn, situation, targetLifeRecord);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			TargetLifePredictionForRandomSelect(target, owner, field, playPtn, situation, targetLifeRecord);
			break;
		}
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (base.SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			LifeRecord lifeRecord = lifeList[i];
			if (!aIVirtualCard.IsLeader && AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation) && !aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
			{
				lifeRecord.CurrentLife = 0;
				if (aIVirtualCard.IsDestroyByBanish)
				{
					situation.RegisterOwnBanishedCard(aIVirtualCard);
				}
				else
				{
					situation.RegisterOwnDestroyedCard(aIVirtualCard);
				}
			}
		}
	}

	private void TargetLifePredictionForAllSelect(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (!target.IsLeader && AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation) && !target.IsIndependent && !target.IsIndestructible)
		{
			targetLifeRecord.CurrentLife = 0;
		}
	}

	private void TargetLifePredictionForRandomSelect(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (!target.IsIndependent && !target.IsIndestructible)
		{
			List<AIVirtualCard> targetsFromField = GetTargetsFromField(owner, field, playPtn, situation);
			int selectCount = GetSelectCount(owner, field, playPtn, situation);
			if (targetsFromField.Count <= selectCount && targetsFromField.Contains(target))
			{
				targetLifeRecord.CurrentLife = 0;
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override AIRemovalType GetRemovalType()
	{
		return AIRemovalType.Destroy;
	}
}
