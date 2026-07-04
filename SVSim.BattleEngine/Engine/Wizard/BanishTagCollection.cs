using System.Collections.Generic;

namespace Wizard;

public class BanishTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTypes = new AIPlayTagType[1] { AIPlayTagType.BanishAttachTag };

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTypes;

	public BanishTagCollection()
		: base(TagCollectionType.WhenBanish)
	{
	}

	private BanishTagCollection(BanishTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new BanishTagCollection(this);
	}

	public void RegisterExecutingTagActions(AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, field, playPtn, situation);
		if (conditionPassedIndexList != null && conditionPassedIndexList.Count > 0)
		{
			situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Banish).AddExecutingAction(delegate
			{
				Execute(tagOwner, conditionPassedIndexList, field, playPtn, situation);
			});
		}
	}

	public void Execute(AIVirtualCard tagOwner, List<int> conditionPassedIndexList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag || conditionPassedIndexList == null || conditionPassedIndexList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < conditionPassedIndexList.Count; i++)
		{
			if (base.TagList.Count > conditionPassedIndexList[i])
			{
				base.TagList[conditionPassedIndexList[i]].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}
}
