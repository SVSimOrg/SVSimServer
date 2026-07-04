using System.Collections.Generic;

namespace Wizard;

public class AIDamageModifierCollection
{
	private List<AIDamageModifierInfo> _modifierList;

	public AIDamageModifierCollection Clone(AIVirtualField field)
	{
		AIDamageModifierCollection aIDamageModifierCollection = new AIDamageModifierCollection();
		if (_modifierList != null)
		{
			for (int i = 0; i < _modifierList.Count; i++)
			{
				AIDamageModifierInfo aIDamageModifierInfo = _modifierList[i];
				AIVirtualCard aIVirtualCard = aIDamageModifierInfo.FindCloneTarget(field);
				if (aIVirtualCard != null)
				{
					aIDamageModifierCollection.AddDamageModifierInfo(aIDamageModifierInfo.Clone(field, aIVirtualCard));
				}
			}
		}
		return aIDamageModifierCollection;
	}

	public int CalcModifiedDamage(AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard damageOwner, int baseDamage)
	{
		if (_modifierList == null || _modifierList.Count <= 0)
		{
			return baseDamage;
		}
		int num = baseDamage;
		for (int i = 0; i < _modifierList.Count; i++)
		{
			AIDamageModifierInfo aIDamageModifierInfo = _modifierList[i];
			if (aIDamageModifierInfo.IsLegalDamageOwner(damageOwner, playPtn, situation))
			{
				num = aIDamageModifierInfo.GetModifiedDamage(field, playPtn, situation, num);
			}
		}
		return num;
	}

	public void AddDamageModifierInfo(AIDamageModifierInfo info)
	{
		_modifierList = AIParamQuery.AddElementToList(info, _modifierList);
	}

	public void DepriveDamageModifierInfo(AIVirtualCard owner, int textHash)
	{
		if (_modifierList == null || _modifierList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _modifierList.Count; i++)
		{
			if (_modifierList[i].IsMatch(owner, textHash))
			{
				_modifierList.RemoveAt(i);
				break;
			}
		}
	}
}
