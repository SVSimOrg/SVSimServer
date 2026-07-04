using System.Collections.Generic;

namespace Wizard;

public class AIEvolvedSkill : AIScriptArgumentExpressions
{
	private readonly int SKILL_TYPE_ARG_INDEX;

	public AIScriptTokenArgType SkillType { get; private set; }

	public AIEvolvedSkill(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count > 0)
		{
			AIScriptTokenBase aIScriptTokenBase = _exprList[SKILL_TYPE_ARG_INDEX].TokenList[0];
			if (aIScriptTokenBase is AIScriptArgumentToken)
			{
				SkillType = ((AIScriptArgumentToken)aIScriptTokenBase).ArgumentType;
			}
			else
			{
				SkillType = AIScriptTokenArgType.NONE;
			}
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		AISkillSimulationUtility.GiveSkill(tagOwner, field, SkillType);
	}
}
