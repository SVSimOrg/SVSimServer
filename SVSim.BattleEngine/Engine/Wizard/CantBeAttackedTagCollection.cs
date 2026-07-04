using System.Collections.Generic;

namespace Wizard;

public class CantBeAttackedTagCollection : TagCollection
{
	public CantBeAttackedTagCollection()
		: base(TagCollectionType.CantBeAttacked)
	{
	}

	private CantBeAttackedTagCollection(CantBeAttackedTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new CantBeAttackedTagCollection(this);
	}

	public bool CantBeAttacked(AIVirtualCard owner, AIVirtualCard target, List<int> playPtn, AIVirtualField field)
	{
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag == null || aIPlayTag.Type != AIPlayTagType.CantBeAttacked)
			{
				continue;
			}
			AIScriptTokenArgType aIScriptTokenArgType = (AIScriptTokenArgType)aIPlayTag.EvalArg(owner, playPtn, field, null);
			int num = (int)aIPlayTag.EvalArg(owner, playPtn, field, null, 1);
			switch (aIScriptTokenArgType)
			{
			case AIScriptTokenArgType.LIFE_INF:
				if (target.Life < num)
				{
					return true;
				}
				break;
			case AIScriptTokenArgType.ATK_INF:
				if (target.Attack < num)
				{
					return true;
				}
				break;
			case AIScriptTokenArgType.LIFE_SUP:
				if (target.Life > num)
				{
					return true;
				}
				break;
			case AIScriptTokenArgType.ATK_SUP:
				if (target.Attack > num)
				{
					return true;
				}
				break;
			case AIScriptTokenArgType.ATK_EQL:
				if (target.Attack == num)
				{
					return true;
				}
				break;
			case AIScriptTokenArgType.LIFE_EQL:
				if (target.Life == num)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}
}
