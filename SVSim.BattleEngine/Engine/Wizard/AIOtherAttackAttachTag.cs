using System.Collections.Generic;

namespace Wizard;

public class AIOtherAttackAttachTag : AIWhenAttackSelfAndOtherTagArgument
{
	private AIScriptTokenArgType _removeTiming;

	private AIScriptTokenArgType _selectType;

	public AIPlayTag Tag { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherAttackAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2]);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0 && _selectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, Tag, _removeTiming, situation);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		list.AddRange(field.CardListSet.AllReferableCards);
		list.AddRange(field.GetEnemyHandCardList());
		return list;
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
