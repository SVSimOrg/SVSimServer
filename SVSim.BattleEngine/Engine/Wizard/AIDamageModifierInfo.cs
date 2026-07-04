using System.Collections.Generic;

namespace Wizard;

public class AIDamageModifierInfo
{
	private int _textHash;

	private AIVirtualCard _owner;

	private List<AIScriptTokenBase> _damageOwnerFilter;

	private AIScriptTokenArgType _calcType;

	private AIPolishConvertedExpression _optionalDamageValue;

	private static AIVirtualField.AIVirtualFieldSearchCardOption _searchOptionForClone = new AIVirtualField.AIVirtualFieldSearchCardOption
	{
		IsSearchFromDeck = false,
		IsOutputCannotFindError = false,
		IsSearchFromBeforeLatestActionDeck = false,
		OptionalSearchRange = BattleCardRealTargetInformation.TargetRange.Default
	};

	public AIDamageModifierInfo(AIVirtualCard owner, List<AIScriptTokenBase> filters, AIScriptTokenArgType calcType, AIPolishConvertedExpression optionalDamage, int textHash)
	{
		_textHash = textHash;
		_owner = owner;
		_damageOwnerFilter = filters;
		_calcType = calcType;
		_optionalDamageValue = optionalDamage;
	}

	public AIDamageModifierInfo Clone(AIVirtualField field, AIVirtualCard newOwner)
	{
		return new AIDamageModifierInfo(newOwner, _damageOwnerFilter, _calcType, _optionalDamageValue, _textHash);
	}

	public bool IsLegalDamageOwner(AIVirtualCard damageOwner, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.CheckMatchTargetFiltering(damageOwner, null, _damageOwnerFilter, playPtn, _owner, situation);
	}

	public int GetModifiedDamage(AIVirtualField field, List<int> playPtn, AISituationInfo situation, int baseDamage)
	{
		int num = baseDamage;
		int num2 = (int)_optionalDamageValue.EvalArg(_owner, playPtn, field, situation);
		switch (_calcType)
		{
		case AIScriptTokenArgType.ADD:
			num += num2;
			break;
		case AIScriptTokenArgType.SET:
			num = num2;
			break;
		}
		return num;
	}

	public bool IsMatch(AIVirtualCard owner, int textHash)
	{
		if (_owner.IsSameCard(owner))
		{
			return _textHash == textHash;
		}
		return false;
	}

	public AIVirtualCard FindCloneTarget(AIVirtualField field)
	{
		return field.SearchVirtualCard(_owner, _searchOptionForClone);
	}
}
