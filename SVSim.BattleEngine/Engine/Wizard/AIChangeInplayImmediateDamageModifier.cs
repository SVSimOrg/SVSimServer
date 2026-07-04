using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayImmediateDamageModifier : AIWhenChangeInplayTagArgument
{
	private static readonly AIScriptTokenArgType[] _legalOptionTypes = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ADD,
		AIScriptTokenArgType.SET
	};

	private AIScriptTokenArgType _modifyOption;

	private AIPolishConvertedExpression _modifyValue;

	protected override int SELECT_TYPE_OFFSET => -1;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIChangeInplayImmediateDamageModifier(string text)
		: base(text, isImmediate: true)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_modifyOption = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], _legalOptionTypes);
		_modifyValue = _exprList[_exprList.Count - 1];
	}

	protected override void InitSelectType()
	{
		base.SelectType = AIScriptTokenArgType.NONE;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISkillProcessInformation processInfo, AISituationInfo situation = null)
	{
		ChangeInplayTagStartProcess(null, field, tagOwner, playPtn, situation);
	}

	public override void Stop(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISkillProcessInformation processInfo, AISituationInfo situation)
	{
		ChangeInplayTagStopProcess(null, field, tagOwner, playPtn, situation);
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		AIDamageModifierInfo info = new AIDamageModifierInfo(tagOwner, base.Filters, _modifyOption, _modifyValue, base.TextHash);
		field.DamageModifierCollection.AddDamageModifierInfo(info);
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		field.DamageModifierCollection.DepriveDamageModifierInfo(tagOwner, base.TextHash);
	}

	public override void ExecuteWhenRemove(AIVirtualCard tagOwner, AIVirtualField field, AIPlayTag removingTag)
	{
		field.DamageModifierCollection.DepriveDamageModifierInfo(tagOwner, base.TextHash);
	}
}
