using System.Collections.Generic;

namespace Wizard;

public class AIGameStartAttachTag : AIFiltersAndSelectTypeArgument
{
	private AIPlayTag _attachTag;

	private AIScriptTokenArgType _removeTiming;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIGameStartAttachTag(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		base.InitExpressions(list[0]);
		_removeTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		if (list.Count <= AIPlayTag.TAG_WORDS_LENTGH)
		{
			AIConsoleUtility.LogError("AIGameStartAttachTag error!! Tag is not completed!!!!!");
			_attachTag = null;
		}
		else
		{
			_attachTag = AIPlayTagInitializingUtility.CreateAIPlayTagFromWords(list[1], list[2], list[3]);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (_attachTag == null)
		{
			return;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIAttachTagSimulationUtility.SimulateAttachTagToAll(targetsFromField, tagOwner, _attachTag, _removeTiming, situation);
			}
			else
			{
				AIConsoleUtility.LogError($"GameStartAttachTag unsupported selectType=={base.SelectType}");
			}
		}
	}
}
