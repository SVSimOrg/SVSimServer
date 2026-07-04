using System.Collections.Generic;
using Wizard.Battle.UI;

namespace Wizard;

public class AITurnEndBanAttack : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private CantAttackType _banAttackType;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int BAN_ATTACK_TYPE_ARG_OFFSET = 2;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndBanAttack(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		_banAttackType = AIPlayTagInitializingUtility.CreateBanAttackType(_exprList[_exprList.Count - BAN_ATTACK_TYPE_ARG_OFFSET]);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIBanAttackSimulationUtility.BanAttackAll(targetsFromField, _banAttackType);
		}
	}
}
