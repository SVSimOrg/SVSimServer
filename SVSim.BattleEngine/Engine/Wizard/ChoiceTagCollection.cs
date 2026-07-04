using System.Collections.Generic;

namespace Wizard;

public class ChoiceTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[4]
	{
		AIPlayTagType.EvoChoice,
		AIPlayTagType.PlayChoice,
		AIPlayTagType.FanfareChoice,
		AIPlayTagType.ChoiceBrave
	};

	private List<AIPlayTag> _evoChoiceList;

	private List<AIPlayTag> _whenPlayChoiceList;

	private AIPlayTag _choiceBraveTag;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public ChoiceTagCollection()
		: base(TagCollectionType.Choice)
	{
		_evoChoiceList = null;
		_whenPlayChoiceList = null;
		_choiceBraveTag = null;
	}

	private ChoiceTagCollection(ChoiceTagCollection param)
		: base(param)
	{
		_evoChoiceList = AIPlayTagInitializingUtility.CloneTagList(param._evoChoiceList);
		_whenPlayChoiceList = AIPlayTagInitializingUtility.CloneTagList(param._whenPlayChoiceList);
		_choiceBraveTag = param._choiceBraveTag;
	}

	public override TagCollection Clone()
	{
		return new ChoiceTagCollection(this);
	}

	public AIVirtualTargetSelectInfo GetSelectInfo(AIVirtualCard owner, AIVirtualField field, AIVirtualTargetSelectAction situation)
	{
		List<AIPlayTag> list = null;
		bool flag = false;
		switch (situation.ActionType)
		{
		case AIOperationType.PLAY:
			if (owner.IsLeader)
			{
				list = AIParamQuery.AddElementToList(_choiceBraveTag, list);
				flag = true;
			}
			else
			{
				list = _whenPlayChoiceList;
			}
			break;
		case AIOperationType.EVOLVE:
			list = _evoChoiceList;
			break;
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list2 = null;
		int num = -1;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		AIPlayTag rule = null;
		for (int i = 0; i < list.Count; i++)
		{
			AIPlayTag aIPlayTag = list[i];
			if (aIPlayTag.CheckCondition(owner, emptyPlayPtn, field, situation))
			{
				AIChoiceTagArgument obj = aIPlayTag.ArgumentExpressions as AIChoiceTagArgument;
				list2 = obj.GetChoiceTargets(owner, field);
				num = obj.GetChoiceCount(owner, field, situation);
				rule = aIPlayTag;
				break;
			}
		}
		if (!flag && (list2 == null || list2.Count <= 0 || num <= 0))
		{
			return null;
		}
		return new AIVirtualTargetSelectInfo(num, list2, TargetSelectType.Choice, isForbiddenSelectedTarget: false, rule);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.EvoChoice:
			_evoChoiceList = AIParamQuery.AddElementToList(tag, _evoChoiceList);
			break;
		case AIPlayTagType.PlayChoice:
		case AIPlayTagType.FanfareChoice:
			_whenPlayChoiceList = AIParamQuery.AddElementToList(tag, _whenPlayChoiceList);
			break;
		case AIPlayTagType.ChoiceBrave:
			_choiceBraveTag = tag;
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_evoChoiceList != null)
		{
			_evoChoiceList.Clear();
			_evoChoiceList = null;
		}
		if (_whenPlayChoiceList != null)
		{
			_whenPlayChoiceList.Clear();
			_whenPlayChoiceList = null;
		}
		if (_choiceBraveTag != null)
		{
			_choiceBraveTag = null;
		}
	}
}
