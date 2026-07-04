using System.Collections.Generic;

namespace Wizard;

public class AIEvoBuff : AIEvoTagArgument
{
	private AIPolishConvertedExpression _attackBuffArgument;

	private AIPolishConvertedExpression _lifeBuffArgument;

	private readonly int ATTACK_BUFF_ARG_OFFSET = 3;

	private readonly int LIFE_BUFF_ARG_OFFSET = 2;

	private readonly int PERM_OR_TEMP_ARG_OFFSET = 1;

	public AIScriptTokenArgType PermOrTemp { get; private set; }

	protected override bool IsSelfTargetForbidden => false;

	protected override int SELECT_TYPE_OFFSET => 4;

	public AIEvoBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackBuffArgument = _exprList[_exprList.Count - ATTACK_BUFF_ARG_OFFSET];
		_lifeBuffArgument = _exprList[_exprList.Count - LIFE_BUFF_ARG_OFFSET];
		PermOrTemp = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - PERM_OR_TEMP_ARG_OFFSET], AIBuffEvaluationUtility.LEGAL_TEMP_OR_PERM_ARGUMENTS);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attackBuffArgument, _lifeBuffArgument);
			bool isTemp = PermOrTemp == AIScriptTokenArgType.TEMP;
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, isTemp, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AIBuffSimulationUtility.BuffTarget_old(situation, targetsFromField, field, playPtn, base.SelectType, buffExecutingInfo_old, isTemp);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (!IsCertainlyIncludeTarget(owner, candidate, situation))
		{
			return false;
		}
		AIVirtualField selfField = owner.SelfField;
		return (int)_lifeBuffArgument.EvalArg(owner, selfField.BestPlayPtn, selfField, situation) + candidate.Life <= 0;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		bool isAttackEffective = _attackBuffArgument != null && !_attackBuffArgument.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}
}
