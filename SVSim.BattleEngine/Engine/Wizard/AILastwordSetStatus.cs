using System.Collections.Generic;

namespace Wizard;

public class AILastwordSetStatus : AIFiltersAndSelectTypeArgument
{
	private AIPolishConvertedExpression _attackArgument;

	private AIPolishConvertedExpression _lifeArgument;

	private readonly int ATTACK_ARG_OFFSET = 2;

	private readonly int LIFE_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 3;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AILastwordSetStatus(string text)
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
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.SetStatusAll(targetsFromField, attack, life, situation);
			}
			else
			{
				AIConsoleUtility.LogError("AILastwordSetStatus Error!! SelecyType " + base.SelectType.ToString() + " is illegal!!!!!");
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
