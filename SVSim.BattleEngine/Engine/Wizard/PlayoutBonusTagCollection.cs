using System.Collections.Generic;

namespace Wizard;

public class PlayoutBonusTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[2]
	{
		AIPlayTagType.PlayoutAttackBonus,
		AIPlayTagType.PlayoutDamageBonus
	};

	private List<AIPlayTag> _playoutAttackBonusTags;

	private List<AIPlayTag> _playoutDamageBonusTags;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasPlayoutAttackBonus
	{
		get
		{
			if (_playoutAttackBonusTags != null)
			{
				return _playoutAttackBonusTags.Count > 0;
			}
			return false;
		}
	}

	public bool HasPlayoutDamageBonus
	{
		get
		{
			if (_playoutDamageBonusTags != null)
			{
				return _playoutDamageBonusTags.Count > 0;
			}
			return false;
		}
	}

	public PlayoutBonusTagCollection()
		: base(TagCollectionType.PlayoutBonus)
	{
	}

	private PlayoutBonusTagCollection(PlayoutBonusTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasPlayoutAttackBonus)
		{
			_playoutAttackBonusTags = new List<AIPlayTag>(tagCollection._playoutAttackBonusTags);
		}
		if (tagCollection.HasPlayoutDamageBonus)
		{
			_playoutDamageBonusTags = new List<AIPlayTag>(tagCollection._playoutDamageBonusTags);
		}
	}

	public override TagCollection Clone()
	{
		return new PlayoutBonusTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.PlayoutAttackBonus:
			if (_playoutAttackBonusTags == null)
			{
				_playoutAttackBonusTags = new List<AIPlayTag>();
			}
			_playoutAttackBonusTags.Add(tag);
			break;
		case AIPlayTagType.PlayoutDamageBonus:
			if (_playoutDamageBonusTags == null)
			{
				_playoutDamageBonusTags = new List<AIPlayTag>();
			}
			_playoutDamageBonusTags.Add(tag);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HasPlayoutAttackBonus)
		{
			_playoutAttackBonusTags.Clear();
		}
		if (HasPlayoutDamageBonus)
		{
			_playoutDamageBonusTags.Clear();
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_playoutAttackBonusTags, tag))
		{
			RemoveTagFromList(_playoutDamageBonusTags, tag);
		}
	}

	public int GetPlayoutAttackBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !HasPlayoutAttackBonus)
		{
			return 0;
		}
		return GetPlayoutBonus(tagOwner, playPtn, _playoutAttackBonusTags, situation);
	}

	public int GetPlayoutDamageBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || !HasPlayoutDamageBonus)
		{
			return 0;
		}
		return GetPlayoutBonus(tagOwner, playPtn, _playoutDamageBonusTags, situation);
	}

	private int GetPlayoutBonus(AIVirtualCard tagOwner, List<int> playPtn, List<AIPlayTag> tagList, AISituationInfo situation)
	{
		int num = 0;
		AIVirtualField field = tagOwner.SelfField;
		List<AIPlayTag> list = tagList.FindAll((AIPlayTag tag) => tag.CheckCondition(tagOwner, playPtn, field, situation));
		if (list.Count > 0)
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				num += (int)list[num2].EvalArg(tagOwner, playPtn, field, situation);
			}
		}
		return num;
	}
}
