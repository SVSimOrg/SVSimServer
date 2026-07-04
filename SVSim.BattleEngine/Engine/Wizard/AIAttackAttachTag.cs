using System.Collections.Generic;

namespace Wizard;

public class AIAttackAttachTag : AIWhenAttackOrWhenFightTagArgument
{
	private AIScriptTokenArgType _removeTiming;

	public AIPlayTag Tag { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAttackAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0 && base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, Tag, _removeTiming, situation);
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

	public override AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (Tag != null)
		{
			return Tag.ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
		}
		return null;
	}
}
