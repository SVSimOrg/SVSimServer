using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayFixRemoveType : AIWhenChangeInplayTagArgument
{
	private readonly FixedRemoveType _removeType;

	protected override int SELECT_TYPE_OFFSET => 0;

	public AIChangeInplayFixRemoveType(string text, FixedRemoveType type, bool isImmediate)
		: base(text, isImmediate)
	{
		_removeType = type;
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			FixRemoveType(targets[i]);
		}
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			StopFixRemoveType(targets[i]);
		}
	}

	private void FixRemoveType(AIVirtualCard target)
	{
		switch (_removeType)
		{
		case FixedRemoveType.RemoveByBanish:
			target.GiveRemoveByBanish();
			break;
		case FixedRemoveType.RemoveByDestroy:
			target.GiveRemoveByDestroy();
			break;
		default:
			AIConsoleUtility.LogError("AIChangeInplayFixRemoveType.FixRemoveType() error!! _removeType == " + _removeType);
			break;
		}
	}

	private void StopFixRemoveType(AIVirtualCard target)
	{
		switch (_removeType)
		{
		case FixedRemoveType.RemoveByBanish:
			target.DepriveRemoveByBanish();
			break;
		case FixedRemoveType.RemoveByDestroy:
			target.DepriveRemoveByDestroy();
			break;
		default:
			AIConsoleUtility.LogError("AIChangeInplayFixRemoveType.StopFixRemoveType() error!! _removeType == " + _removeType);
			break;
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
