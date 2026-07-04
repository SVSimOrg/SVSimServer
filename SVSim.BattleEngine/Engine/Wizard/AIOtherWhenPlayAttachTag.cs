using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayAttachTag : AIOtherWhenPlayTagArgument
{

	public AIPlayTag Tag { get; private set; }

	public AIScriptTokenArgType RemoveTiming { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIOtherWhenPlayAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		RemoveTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			AIConsoleUtility.LogError("AIOtherWhenPlayAttachTag error!! Tag is not completed!!!!!");
			Tag = null;
		}
		else
		{
			Tag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targets, tagOwner, Tag, RemoveTiming, situation);
			}
			else
			{
				AIConsoleUtility.LogError("AIOtherWhenPlayAttachTag.Execute() Error!! SelectType=" + SelectType);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
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
