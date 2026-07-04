using System.Collections.Generic;

namespace Wizard;

public class AIEvoSetStatus : AIEvoTagArgument
{
	private AIPolishConvertedExpression _attackArgument;

	private AIPolishConvertedExpression _lifeArgument;

	private readonly int ATTACK_ARG_OFFSET = 2;

	private readonly int LIFE_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIEvoSetStatus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackArgument = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		_lifeArgument = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int attack = (int)_attackArgument.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
			int life = (int)_lifeArgument.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.SetStatusAll(targetsFromField, attack, life, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AISkillSimulationUtility.SetStatusTarget(targetsFromField, base.SelectType, attack, life, situation);
				break;
			default:
				AIConsoleUtility.LogError("AIEvoSetStatus Error!! SelecyType " + base.SelectType.ToString() + " is illegal!!!!!");
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
		return (int)_lifeArgument.EvalArg(owner, selfField.BestPlayPtn, selfField, situation) <= 0;
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

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: true, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
