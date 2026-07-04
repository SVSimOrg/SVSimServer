using System.Collections.Generic;

namespace Wizard;

public class AINecromanceAttachTag : AIFiltersAndSelectTypeArgument
{

	public AIPlayTag AttachedTag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AINecromanceAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			AttachedTag = null;
		}
		else
		{
			AttachedTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, AttachedTag, RemoveTiming, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				AIDefaultSelectLogicArgument selectLogic = new AIDefaultSelectLogicArgument(null);
				AIAttachTagSimulationUtility.SimulateRandomSelectAttachTag(targetsFromField, 1, tagOwner, field, playPtn, situation, AttachedTag, RemoveTiming, selectLogic);
				break;
			}
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}

	public override AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (AttachedTag != null)
		{
			return AttachedTag.ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
		}
		return null;
	}
}
