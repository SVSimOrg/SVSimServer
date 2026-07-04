using System.Collections.Generic;

namespace Wizard;

public class AIAttackHandBuff : AIWhenAttackOrWhenFightTagArgument
{
	private AIPolishConvertedExpression _attack;

	private AIPolishConvertedExpression _life;

	private readonly int ATTACK_BUFF_ARG_OFFSET = 2;

	private readonly int LIFE_BUFF_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIAttackHandBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attack = _exprList[_exprList.Count - ATTACK_BUFF_ARG_OFFSET];
		_life = _exprList[_exprList.Count - LIFE_BUFF_ARG_OFFSET];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.RANDOM_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: true, isBlockDead);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attack, _life);
			if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				ExecuteRandomSelectHandBuff(targetsFromField, buffExecutingInfo_old, tagOwner, field, playPtn, situation);
			}
		}
	}

	private void ExecuteRandomSelectHandBuff(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		AIVirtualCardRealTargetInformation aIVirtualCardRealTargetInformation = null;
		if (situation.IsLatestAction)
		{
			aIVirtualCardRealTargetInformation = situation.DequeueRealTargetInfo(tagOwner, field);
			if (aIVirtualCardRealTargetInformation == null || aIVirtualCardRealTargetInformation.TargetList.Count <= 0)
			{
				AIConsoleUtility.LogError("AIAttackHandBuff.ExecuteRandomSelectHandBuff error!! Cannot find real target!!!!!! tagOwner.BaseId = " + tagOwner.BaseId);
			}
			else if (IsCandidateLegal(aIVirtualCardRealTargetInformation, candidates, tagOwner.BaseId))
			{
				AIHandBuffSimulationUtility.ExecuteHandBuffAll(aIVirtualCardRealTargetInformation.TargetList, buffInfo, situation);
			}
		}
		else
		{
			AIHandBuffSimulationUtility.ExecuteHandBuffRandom(candidates, buffInfo, playPtn, situation);
		}
	}

	private bool IsCandidateLegal(AIVirtualCardRealTargetInformation realTargets, List<AIVirtualCard> virtualCandidates, int tagOwnerId)
	{
		for (int i = 0; i < realTargets.TargetList.Count; i++)
		{
			AIVirtualCard card = realTargets.TargetList[i];
			bool flag = false;
			for (int j = 0; j < virtualCandidates.Count; j++)
			{
				if (virtualCandidates[j].IsSameCard(card))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AIConsoleUtility.LogError("AIAttackHandBuff.IsCandidateLegal() error!! Candidates does not include target!!!!! tagOwner.BaseId == " + tagOwnerId);
				return false;
			}
		}
		return true;
	}
}
