using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayRecoverAttackableCount : AIWhenPlayTagArgument
{
	protected override bool _isSelectCountImplemented => true;

	public AIWhenPlayRecoverAttackableCount(string text)
		: base(text)
	{
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothClassAndInplayCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.TARGET_SELECT)
		{
			ExecuteTargetSelectRecoverAttackableCount(targetsFromField, tagOwner, field, playPtn, situation);
		}
	}

	private void ExecuteTargetSelectRecoverAttackableCount(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			AIRecoverAttackableCountUtility.RecoverAttackableCountTarget(base.SelectType, situation);
			return;
		}
		int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
		AIRecoverAttackableCountUtility.RecoverAttackableCountTargetPrediction(targets, selectCount);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.TARGET_SELECT };
	}
}
