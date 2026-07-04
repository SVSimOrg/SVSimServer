using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayCannotAttack : AIWhenChangeInplayTagArgument
{
	private AICannotAttackInformation _info;

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => 0;

	public AIChangeInplayCannotAttack(string text, bool isImmediate)
		: base(text, isImmediate)
	{
		_info = null;
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.ALL_SELECT;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return GetCandidateRange(field);
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_info == null)
		{
			_info = new AICannotAttackInformation(base.Filters);
		}
		tagOwner.AddCannotAttackInformation(_info);
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		tagOwner.RemoveCannotAttackInformation(_info);
	}
}
