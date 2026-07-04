using System.Collections.Generic;

namespace Wizard;

public class EmoteTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[6]
	{
		AIPlayTagType.EmoteOnTurnEnd,
		AIPlayTagType.EmoteOnPlay,
		AIPlayTagType.EmoteOnAtk,
		AIPlayTagType.EmoteOnEvo,
		AIPlayTagType.EmoteOnDestroy,
		AIPlayTagType.ForceEmoteOnDestroy
	};

	public static int INVALID_EMOTE_CATEGORY = -1;

	private List<AIPlayTag> _turnEndTagList;

	private List<AIPlayTag> _playTagList;

	private List<AIPlayTag> _attackTagList;

	private List<AIPlayTag> _evolveTagList;

	private List<AIPlayTag> _destroyTagList;

	private List<AIPlayTag> _destroyForceTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public int EmoteOnTurnEndCount
	{
		get
		{
			if (_turnEndTagList != null)
			{
				return _turnEndTagList.Count;
			}
			return 0;
		}
	}

	public EmoteTagCollection()
		: base(TagCollectionType.Emote)
	{
		_turnEndTagList = null;
		_playTagList = null;
		_attackTagList = null;
		_evolveTagList = null;
		_destroyTagList = null;
		_destroyForceTagList = null;
	}

	private EmoteTagCollection(EmoteTagCollection param)
		: base(param)
	{
		_turnEndTagList = param.CreateEmoteOnTurnEndListClone();
		_playTagList = param.CreateEmoteOnPlayListClone();
		_attackTagList = param.CreateEmoteOnAttackListClone();
		_evolveTagList = param.CreateEmoteOnEvoListClone();
		_destroyTagList = param.CreateEmoteOnDestroyListClone();
		_destroyForceTagList = param.CreateForceEmoteOnDestroyListClone();
	}

	public override TagCollection Clone()
	{
		return new EmoteTagCollection(this);
	}

	public List<AIPlayTag> CreateEmoteOnTurnEndListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_turnEndTagList);
	}

	public List<AIPlayTag> CreateEmoteOnPlayListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_playTagList);
	}

	public List<AIPlayTag> CreateEmoteOnAttackListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_attackTagList);
	}

	public List<AIPlayTag> CreateEmoteOnEvoListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_evolveTagList);
	}

	public List<AIPlayTag> CreateEmoteOnDestroyListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_destroyTagList);
	}

	public List<AIPlayTag> CreateForceEmoteOnDestroyListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_destroyForceTagList);
	}

	public int GetEmoteOnTurnEndCategory(AIVirtualCard tagOwner, bool isOwnerTurn)
	{
		if (!base.HasTag || EmoteOnTurnEndCount <= 0)
		{
			return INVALID_EMOTE_CATEGORY;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		for (int i = 0; i < _turnEndTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _turnEndTagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, emptyPlayPtn, selfField, null))
			{
				int emoteIdIfSideIsCorrect = (aIPlayTag.ArgumentExpressions as AIEmoteOnTurnTransition).GetEmoteIdIfSideIsCorrect(isOwnerTurn);
				if (emoteIdIfSideIsCorrect >= 0)
				{
					return emoteIdIfSideIsCorrect;
				}
			}
		}
		return INVALID_EMOTE_CATEGORY;
	}

	public int GetEmoteCategory(AIVirtualCard owner, AIPlayTagType emoteType)
	{
		if (!base.HasTag)
		{
			return INVALID_EMOTE_CATEGORY;
		}
		List<AIPlayTag> tagListByType = GetTagListByType(emoteType);
		if (tagListByType == null || tagListByType.Count <= 0)
		{
			return INVALID_EMOTE_CATEGORY;
		}
		AIVirtualField selfField = owner.SelfField;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		for (int i = 0; i < tagListByType.Count; i++)
		{
			AIPlayTag aIPlayTag = tagListByType[i];
			if (aIPlayTag.CheckCondition(owner, emptyPlayPtn, selfField, null))
			{
				int num = (int)aIPlayTag.EvalArg(owner, emptyPlayPtn, selfField, null);
				if (num >= 0)
				{
					return num;
				}
			}
		}
		return INVALID_EMOTE_CATEGORY;
	}

	private List<AIPlayTag> GetTagListByType(AIPlayTagType type)
	{
		return type switch
		{
			AIPlayTagType.EmoteOnTurnEnd => _turnEndTagList, 
			AIPlayTagType.EmoteOnPlay => _playTagList, 
			AIPlayTagType.EmoteOnAtk => _attackTagList, 
			AIPlayTagType.EmoteOnEvo => _evolveTagList, 
			AIPlayTagType.EmoteOnDestroy => _destroyTagList, 
			AIPlayTagType.ForceEmoteOnDestroy => _destroyForceTagList, 
			_ => null, 
		};
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.EmoteOnTurnEnd:
			_turnEndTagList = AIParamQuery.AddElementToList(tag, _turnEndTagList);
			break;
		case AIPlayTagType.EmoteOnPlay:
			_playTagList = AIParamQuery.AddElementToList(tag, _playTagList);
			break;
		case AIPlayTagType.EmoteOnAtk:
			_attackTagList = AIParamQuery.AddElementToList(tag, _attackTagList);
			break;
		case AIPlayTagType.EmoteOnEvo:
			_evolveTagList = AIParamQuery.AddElementToList(tag, _evolveTagList);
			break;
		case AIPlayTagType.EmoteOnDestroy:
			_destroyTagList = AIParamQuery.AddElementToList(tag, _destroyTagList);
			break;
		case AIPlayTagType.ForceEmoteOnDestroy:
			_destroyForceTagList = AIParamQuery.AddElementToList(tag, _destroyForceTagList);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_turnEndTagList != null)
		{
			_turnEndTagList.Clear();
			_turnEndTagList = null;
		}
		if (_playTagList != null)
		{
			_playTagList.Clear();
			_playTagList = null;
		}
		if (_attackTagList != null)
		{
			_attackTagList.Clear();
			_attackTagList = null;
		}
		if (_evolveTagList != null)
		{
			_evolveTagList.Clear();
			_evolveTagList = null;
		}
		if (_destroyTagList != null)
		{
			_destroyTagList.Clear();
			_destroyTagList = null;
		}
		if (_destroyForceTagList != null)
		{
			_destroyForceTagList.Clear();
			_destroyForceTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_turnEndTagList, tag) && !RemoveTagFromList(_playTagList, tag) && !RemoveTagFromList(_attackTagList, tag) && !RemoveTagFromList(_evolveTagList, tag) && !RemoveTagFromList(_destroyTagList, tag))
		{
			RemoveTagFromList(_destroyForceTagList, tag);
		}
	}
}
