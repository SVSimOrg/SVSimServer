using System.Collections.Generic;
using Wizard.Battle.UI;

namespace Wizard;

public class AISummonBanAttack : AIFiltersAndSelectTypeArgument
{
	private CantAttackType _banAttackType;

	private readonly int BAN_ATTACK_TYPE_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AISummonBanAttack(string text)
		: base(text)
	{
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_banAttackType = AIPlayTagInitializingUtility.CreateBanAttackType(_exprList[_exprList.Count - BAN_ATTACK_TYPE_ARG_OFFSET]);
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
