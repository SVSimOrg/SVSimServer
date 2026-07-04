using System.Collections.Generic;
using Wizard.Battle.UI;

namespace Wizard;

public class AIWhenPlayBanAttack : AIWhenPlayTagArgument
{
	private readonly int BAN_ATTACK_TYPE_ARG_OFFSET = 1;

	private CantAttackType _banAttackType;

	protected override bool _isSelectCountImplemented => true;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayBanAttack(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_banAttackType = AIPlayTagInitializingUtility.CreateBanAttackType(_exprList[_exprList.Count - BAN_ATTACK_TYPE_ARG_OFFSET]);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
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
				AIBanAttackSimulationUtility.BanAttackAll(targetsFromField, _banAttackType);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
				AIBanAttackSimulationUtility.BanAttackRandom(targetsFromField, _banAttackType, selectCount);
				break;
			}
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				ExecuteTargetSelectBanAttack(tagOwner, targetsFromField, situation, field, playPtn);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	private void ExecuteTargetSelectBanAttack(AIVirtualCard owner, List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn)
	{
		if (situation != null)
		{
			if (!situation.IsTargetExists(base.SelectType))
			{
				int selectCount = GetSelectCount(owner, field, playPtn, situation);
				AIBanAttackSimulationUtility.BanAttackTargetPrediction(AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(targets, owner, playPtn), _banAttackType, selectCount);
			}
			else
			{
				AIBanAttackSimulationUtility.BanAttackTarget(situation, _banAttackType, base.SelectType);
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
