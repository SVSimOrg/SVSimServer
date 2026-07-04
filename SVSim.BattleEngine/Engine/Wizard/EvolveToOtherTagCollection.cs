using System.Collections.Generic;

namespace Wizard;

public class EvolveToOtherTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[1] { AIPlayTagType.EvolveToOther };

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public EvolveToOtherTagCollection()
		: base(TagCollectionType.EvolveToOther)
	{
	}

	private EvolveToOtherTagCollection(EvolveToOtherTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new EvolveToOtherTagCollection(this);
	}

	public void PreparateBeforeEvolve(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				aIPlayTag.ArgumentExpressions.Execute(owner, field, playPtn, situation);
				break;
			}
		}
	}
}
