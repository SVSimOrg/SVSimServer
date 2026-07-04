using System.Collections.Generic;

namespace Wizard;

public class AISummonBuff : AIFiltersArgument
{

	private readonly AIScriptTokenArgType[] _legalSelectTypeArgs = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ALL_SELECT,
		AIScriptTokenArgType.FIRST_SELECT
	};

	public AIPolishConvertedExpression AtkBuff { get; private set; }

	public AIPolishConvertedExpression LifeBuff { get; private set; }

	public bool IsTemp { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 4;

	public AIScriptTokenArgType SelectTypeArg { get; private set; }

	public AISummonBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AtkBuff = _exprList[_exprList.Count - 3];
		LifeBuff = _exprList[_exprList.Count - 2];
		AIScriptTokenArgType argumentType = ((AIScriptArgumentToken)_exprList[_exprList.Count - 1].TokenList[0]).ArgumentType;
		if (argumentType != AIScriptTokenArgType.PERM)
		{
			_ = 138;
		}
		IsTemp = argumentType == AIScriptTokenArgType.TEMP;
		SelectTypeArg = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 4], _legalSelectTypeArgs);
		if (SelectTypeArg != AIScriptTokenArgType.ALL_SELECT && SelectTypeArg != AIScriptTokenArgType.FIRST_SELECT)
		{
			AIConsoleUtility.LogError("AISummonBuff SelectType Error!! ALL_SELECT or FIRST_SELECT =" + SelectTypeArg);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation == null || situation.Actor == null)
		{
			return;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, AtkBuff, LifeBuff);
			switch (SelectTypeArg)
			{
			case AIScriptTokenArgType.FIRST_SELECT:
				AIBuffSimulationUtility.BuffFirst_old(targetsFromField, field, buffExecutingInfo_old, IsTemp, playPtn, situation);
				break;
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, IsTemp, playPtn, situation);
				break;
			default:
				AIConsoleUtility.LogError(string.Format("AISummonBuff : ILlegal SelectType ({0}) owner ({1})", SelectTypeArg, (tagOwner != null) ? tagOwner.CardName : ""));
				break;
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !AtkBuff.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
