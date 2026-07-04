using System.Collections.Generic;

namespace Wizard;

public class BattleCardRealTargetInformation
{
	public enum TargetRange
	{
		Default,
		DestroyedCardList
	}

	private class SingleSkillTargetInformation
	{
		public List<BattleCardBase> TargetList;

		public TargetRange VirtualCardSearchRange;

		private bool CheckCreateTokenIdCondition(AITokenType type)
		{
			if (type == AITokenType.Reanimate)
			{
				bool num = VirtualCardSearchRange == TargetRange.DestroyedCardList;
				if (!num)
				{
					AIConsoleUtility.LogError("SingleSkillTargetInformation.CheckCreateTokenIdCondition() error!! Reanimate target search range = " + VirtualCardSearchRange);
				}
				return num;
			}
			return true;
		}

		public List<int> CreateTokenIdList(AIVirtualCard owner, AITokenType tokenType)
		{
			if (!CheckCreateTokenIdCondition(tokenType))
			{
				return null;
			}
			List<int> list = new List<int>();
			AITokenManager tokenManager = owner.SelfField.AI.tokenManager;
			for (int i = 0; i < TargetList.Count; i++)
			{
				BattleCardBase battleCardBase = TargetList[i];
				int baseCardId = battleCardBase.BaseParameter.BaseCardId;
				bool isAlly = battleCardBase.IsPlayer == owner.IsPlayer;
				list.Add(baseCardId);
				tokenManager.AddTokenFromId(baseCardId, isAlly, tokenType == AITokenType.Choice);
			}
			return list;
		}
	}

	public BattleCardBase SkillOwner;

	private List<SingleSkillTargetInformation> _targetInformationList;

	public bool HasAnyTarget
	{
		get
		{
			if (_targetInformationList != null)
			{
				return _targetInformationList.Count > 0;
			}
			return false;
		}
	}

	public BattleCardRealTargetInformation(BattleCardBase owner)
	{
		SkillOwner = owner;
		_targetInformationList = null;
	}

	public void AddTargetList(List<BattleCardBase> newAddedList, ISkillTargetFilter targetFilter)
	{
		SingleSkillTargetInformation element = new SingleSkillTargetInformation
		{
			TargetList = newAddedList,
			VirtualCardSearchRange = GetTargetRange(targetFilter)
		};
		_targetInformationList = AIParamQuery.AddElementToList(element, _targetInformationList);
	}

	private TargetRange GetTargetRange(ISkillTargetFilter targetFilter)
	{
		if (targetFilter is SkillTargetDestroyedCardListFilter)
		{
			return TargetRange.DestroyedCardList;
		}
		return TargetRange.Default;
	}

	public List<int> DequeueFirstTargetInfoAndCreateTokenIdList(AIVirtualCard owner, AITokenType tokenType)
	{
		if (!HasAnyTarget)
		{
			return null;
		}
		SingleSkillTargetInformation singleSkillTargetInformation = _targetInformationList[0];
		_targetInformationList.RemoveAt(0);
		return singleSkillTargetInformation.CreateTokenIdList(owner, tokenType);
	}

	public bool IsTarget(AIVirtualCard virtualTarget)
	{
		if (!HasAnyTarget)
		{
			return false;
		}
		for (int i = 0; i < _targetInformationList.Count; i++)
		{
			SingleSkillTargetInformation singleSkillTargetInformation = _targetInformationList[i];
			int num = 0;
			while (num < singleSkillTargetInformation.TargetList.Count)
			{
				if (virtualTarget.IsEqual(singleSkillTargetInformation.TargetList[i]))
				{
					return true;
				}
				i++;
			}
		}
		return false;
	}

	public AIVirtualCardRealTargetInformation CreateAIVirtualTargetInformation(AIVirtualField field, AIVirtualCard owner, AIVirtualField.AIVirtualFieldSearchCardOption searchOption)
	{
		if (!HasAnyTarget)
		{
			return null;
		}
		List<AIVirtualCard> list = null;
		for (int i = 0; i < _targetInformationList.Count; i++)
		{
			SingleSkillTargetInformation singleSkillTargetInformation = _targetInformationList[i];
			searchOption.OptionalSearchRange = singleSkillTargetInformation.VirtualCardSearchRange;
			for (int j = 0; j < singleSkillTargetInformation.TargetList.Count; j++)
			{
				AIVirtualCard aIVirtualCard = field.SearchVirtualCard(singleSkillTargetInformation.TargetList[j], searchOption);
				if (aIVirtualCard != null)
				{
					list = AIParamQuery.AddElementToList(aIVirtualCard, list);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			return new AIVirtualCardRealTargetInformation(owner, list);
		}
		return null;
	}
}
