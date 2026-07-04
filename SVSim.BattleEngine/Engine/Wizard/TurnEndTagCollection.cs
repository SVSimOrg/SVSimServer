using System;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class TurnEndTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[21]
	{
		AIPlayTagType.TurnEndMetamorphose,
		AIPlayTagType.TurnEndDamage,
		AIPlayTagType.TurnEndDestroy,
		AIPlayTagType.TurnEndHeal,
		AIPlayTagType.TurnEndBanish,
		AIPlayTagType.TurnEndSetLeaderMaxLife,
		AIPlayTagType.TurnEndDiscard,
		AIPlayTagType.TurnEndBuff,
		AIPlayTagType.TurnEndSubtractCountdown,
		AIPlayTagType.TurnEndToken,
		AIPlayTagType.TurnEndBounce,
		AIPlayTagType.TurnEndAttachTag,
		AIPlayTagType.TurnEndAddDeck,
		AIPlayTagType.TurnEndEvo,
		AIPlayTagType.TurnEndDraw,
		AIPlayTagType.TurnEndRemoveTag,
		AIPlayTagType.TurnEndShield,
		AIPlayTagType.TurnEndDamageClip,
		AIPlayTagType.TurnEndDamageCut,
		AIPlayTagType.TurnEndGuard,
		AIPlayTagType.TurnEndBanAttack
	};

	private List<AIPlayTag> _turnEndAddDeckTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public List<AIPlayTag> AllyTurnTagList { get; private set; }

	public List<AIPlayTag> EnemyTurnTagList { get; private set; }

	public List<AIPlayTag> TurnEndDamageList { get; private set; }

	public List<AIPlayTag> TurnEndTokenList { get; private set; }

	public bool HasTurnEndDamage
	{
		get
		{
			if (TurnEndDamageList != null)
			{
				return TurnEndDamageList.Count > 0;
			}
			return false;
		}
	}

	public bool HasTurnEndToken
	{
		get
		{
			if (TurnEndTokenList != null)
			{
				return TurnEndTokenList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAllyTurnTag
	{
		get
		{
			if (AllyTurnTagList != null)
			{
				return AllyTurnTagList.Count > 0;
			}
			return false;
		}
	}

	public bool HasEnemyTurnTag
	{
		get
		{
			if (EnemyTurnTagList != null)
			{
				return EnemyTurnTagList.Count > 0;
			}
			return false;
		}
	}

	public TurnEndTagCollection()
		: base(TagCollectionType.WhenTurnEnd)
	{
		TurnEndDamageList = null;
		AllyTurnTagList = null;
		EnemyTurnTagList = null;
		_turnEndAddDeckTagList = null;
	}

	private TurnEndTagCollection(TurnEndTagCollection param)
		: base(param)
	{
		if (param.HasTurnEndDamage)
		{
			TurnEndDamageList = new List<AIPlayTag>(param.TurnEndDamageList);
		}
		if (param.HasTurnEndToken)
		{
			TurnEndTokenList = new List<AIPlayTag>(param.TurnEndTokenList);
		}
		if (param.HasAllyTurnTag)
		{
			AllyTurnTagList = new List<AIPlayTag>(param.AllyTurnTagList);
		}
		if (param.HasEnemyTurnTag)
		{
			EnemyTurnTagList = new List<AIPlayTag>(param.EnemyTurnTagList);
		}
		if (param._turnEndAddDeckTagList != null)
		{
			_turnEndAddDeckTagList = new List<AIPlayTag>(param._turnEndAddDeckTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new TurnEndTagCollection(this);
	}

	public void RegisterConditionPassedTagActions(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualTurnEndInfo situation, AISkillProcessInformation processInfo)
	{
		if (!base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags);
			}
		}
		if (passedConditionTags != null && passedConditionTags.Count > 0)
		{
			processInfo.AddExecutingAction(delegate
			{
				Execute(tagOwner, field, playPtn, passedConditionTags, situation);
			});
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<AIPlayTag> passedContitionTags, AISituationInfo situation)
	{
		if (tagOwner.IsDead || passedContitionTags == null || passedContitionTags.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < passedContitionTags.Count; i++)
		{
			AIPlayTag aIPlayTag = passedContitionTags[i];
			if (aIPlayTag.ArgumentExpressions is AITurnEndRemoveTag aITurnEndRemoveTag)
			{
				aITurnEndRemoveTag.Execute(tagOwner, field, situation, aIPlayTag);
			}
			else
			{
				aIPlayTag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}

	public float CalculateEnemyTurnEndTagThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		float num = 0f;
		List<AIPlayTag> list = (tagOwner.IsAlly ? EnemyTurnTagList : AllyTurnTagList);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].CheckCondition(tagOwner, selfField.BestPlayPtn, selfField, selfField.CommonEnemyTurnEndSituation))
			{
				IAITurnEndArgument iAITurnEndArgument = list[i].ArgumentExpressions as IAITurnEndArgument;
				num += iAITurnEndArgument.CalculateThreaten(tagOwner, ref allInplayStatusList);
			}
		}
		return num;
	}

	public int GetTurnEndDamageToAllyLeader(AIVirtualCard tagOwner)
	{
		int num = 0;
		if (HasTurnEndDamage)
		{
			AIVirtualField selfField = tagOwner.SelfField;
			List<int> bestPlayPtn = selfField.BestPlayPtn;
			for (int i = 0; i < TurnEndDamageList.Count; i++)
			{
				AITurnEndDamage obj = TurnEndDamageList[0].ArgumentExpressions as AITurnEndDamage;
				int damage = obj.GetDamage(tagOwner, bestPlayPtn);
				int count = obj.GetCount(tagOwner, bestPlayPtn);
				AIVirtualCard aIVirtualCard = (tagOwner.IsAlly ? selfField.AllyClass : selfField.EnemyClass);
				if (obj.IsTarget(aIVirtualCard, tagOwner, selfField, bestPlayPtn, selfField.CommonAllyTurnEndSituation))
				{
					num += aIVirtualCard.SimulateDamageAmount(damage, isSkillDamage: true, tagOwner.IsSpell) * count;
				}
			}
		}
		return num;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.TurnEndDamage)
		{
			TurnEndDamageList = AIParamQuery.AddElementToList(tag, TurnEndDamageList);
		}
		if (tag.Type == AIPlayTagType.TurnEndToken)
		{
			TurnEndTokenList = AIParamQuery.AddElementToList(tag, TurnEndTokenList);
		}
		if (tag.Type == AIPlayTagType.TurnEndAddDeck)
		{
			_turnEndAddDeckTagList = AIParamQuery.AddElementToList(tag, _turnEndAddDeckTagList);
		}
		if (tag.ArgumentExpressions is IAITurnEndArgument iAITurnEndArgument)
		{
			if (iAITurnEndArgument.IsAllyTurn)
			{
				AllyTurnTagList = AIParamQuery.AddElementToList(tag, AllyTurnTagList);
			}
			else
			{
				EnemyTurnTagList = AIParamQuery.AddElementToList(tag, EnemyTurnTagList);
			}
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HasTurnEndDamage)
		{
			TurnEndDamageList.Clear();
			TurnEndDamageList = null;
		}
		if (HasTurnEndToken)
		{
			TurnEndTokenList.Clear();
			TurnEndTokenList = null;
		}
		if (HasAllyTurnTag)
		{
			AllyTurnTagList.Clear();
			AllyTurnTagList = null;
		}
		if (HasEnemyTurnTag)
		{
			EnemyTurnTagList.Clear();
			EnemyTurnTagList = null;
		}
		if (_turnEndAddDeckTagList != null)
		{
			_turnEndAddDeckTagList.Clear();
			_turnEndAddDeckTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(TurnEndDamageList, tag) && !RemoveTagFromList(TurnEndTokenList, tag) && !RemoveTagFromList(_turnEndAddDeckTagList, tag) && !RemoveTagFromList(AllyTurnTagList, tag))
		{
			RemoveTagFromList(EnemyTurnTagList, tag);
		}
	}

	public static bool IsAllyTurn(List<AIPolishConvertedExpression> exprList, Type argType, int index)
	{
		if (!(exprList[index].TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken))
		{
			AIConsoleUtility.LogError(argType?.ToString() + " error!! IsAllyTurnArg == null!!!!!");
			return true;
		}
		if (aIScriptArgumentToken.ArgumentType != AIScriptTokenArgType.ALLY && aIScriptArgumentToken.ArgumentType != AIScriptTokenArgType.OPPONENT)
		{
			AIConsoleUtility.LogError(argType?.ToString() + " error!! IsAllyTurnArg.ArgumentType == " + aIScriptArgumentToken.ArgumentType.ToString() + "!!!!!");
			return true;
		}
		return aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.ALLY;
	}

	public bool HasTagExecuteWhenAllyTurnEnd(bool isOwnerAlly)
	{
		if (!isOwnerAlly)
		{
			return HasEnemyTurnTag;
		}
		return HasAllyTurnTag;
	}
}
