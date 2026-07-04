using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoBuff : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _attackBuff;

	private AIPolishConvertedExpression _lifeBuff;

	private AIScriptTokenArgType _permOrTemp;

	protected override int SELECT_TYPE_ARG_OFFSET => 4;

	public AIOtherEvoBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackBuff = _exprList[_exprList.Count - 3];
		_lifeBuff = _exprList[_exprList.Count - 2];
		_permOrTemp = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1], AIBuffEvaluationUtility.LEGAL_TEMP_OR_PERM_ARGUMENTS);
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		if (!IsCertainlyIncludeTarget(owner, candidate, situation))
		{
			return false;
		}
		AIVirtualField selfField = owner.SelfField;
		return GetLifeBuff(owner, selfField, selfField.BestPlayPtn, situation) + candidate.Life <= 0;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attackBuff, _lifeBuff);
			bool isTemp = _permOrTemp == AIScriptTokenArgType.TEMP;
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp, playPtn, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBuffSimulationUtility.BuffRandom_old(targets, field, playPtn, situation, buffExecutingInfo_old, isTemp);
				break;
			}
		}
	}

	private int GetLifeBuff(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_lifeBuff == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoBuff error!!! _lifeBuff is null");
			return 0;
		}
		return (int)_lifeBuff.EvalArg(tagOwner, playPtn, field, situation);
	}
}
