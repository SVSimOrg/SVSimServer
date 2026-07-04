using System.Collections.Generic;

namespace Wizard;

public class AISummonAttachTag : AIFiltersArgument
{

	private readonly AIScriptTokenArgType[] _legalSelectTypeArgs = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ALL_SELECT,
		AIScriptTokenArgType.FIRST_SELECT
	};

	public AIPlayTag AttachedTag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIScriptTokenArgType SelectTypeArg { get; private set; }

	public AISummonAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		SelectTypeArg = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], _legalSelectTypeArgs);
		if (SelectTypeArg != AIScriptTokenArgType.ALL_SELECT && SelectTypeArg != AIScriptTokenArgType.FIRST_SELECT)
		{
			AIConsoleUtility.LogError("AISummonAttachTag SelectType Error!! ALL_SELECT or FIRST_SELECT =" + SelectTypeArg);
		}
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
			switch (SelectTypeArg)
			{
			case AIScriptTokenArgType.FIRST_SELECT:
				AIAttachTagSimulationUtility.SimulateAttachTagToSingle(targetsFromField[0], tagOwner, AttachedTag, RemoveTiming, situation);
				break;
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, AttachedTag, RemoveTiming, situation);
				break;
			default:
				AIConsoleUtility.LogError(string.Format("AISummonAttachTag : ILlegal SelectType ({0}) owner ({1})", SelectTypeArg, (tagOwner != null) ? tagOwner.CardName : ""));
				break;
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
