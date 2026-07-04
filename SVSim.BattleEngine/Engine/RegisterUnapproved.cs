using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class RegisterUnapproved
{
	private BattleCardBase _targetCard;

	public List<int> IndexList;

	public bool IsPlayer;

	public List<int> AttachedSkillsPublishCount;

	public NetworkBattleDefine.NetworkCardPlaceState FromPlaceState { get; private set; }

	public NetworkBattleDefine.NetworkCardPlaceState ToPlaceState { get; private set; }

	public int SkillCardIdx { get; private set; }

	public int PublishedActiveSkillCount { get; private set; }

	public int CardId { get; private set; }

	public int Clan { get; private set; }

	public int Cost { get; private set; } = -1;

	public int Movement { get; private set; }

	public List<int> RandomTargetIdx { get; private set; }

	public List<int> SkillKeyCardIdxList { get; private set; }

	public bool IsShortageDeck { get; private set; }

	public bool IsInvoked { get; private set; }

	public RegisterUnapproved(SkillBase skill, BattleCardBase targetCard, NetworkBattleDefine.NetworkCardPlaceState from = NetworkBattleDefine.NetworkCardPlaceState.None, NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.None, int skillMovementNum = 0, bool isCardId = false)
	{
		_targetCard = targetCard;
		IndexList = new List<int>();
		RandomTargetIdx = new List<int>();
		IndexList.Add(targetCard.Index);
		IsPlayer = targetCard.IsPlayer;
		FromPlaceState = from;
		ToPlaceState = to;
		SkillCardIdx = skill.SkillPrm.ownerCard.Index;
		PublishedActiveSkillCount = NetworkBattleGenericTool.GetPublishSkillCount(skill);
		SkillKeyCardIdxList = new List<int>();
		Clan = -1;
		Movement = skillMovementNum;
		AttachedSkillsPublishCount = new List<int>();
		if (isCardId)
		{
			CardId = targetCard.CardId;
			Clan = (int)targetCard.Clan;
			for (int i = 0; i < targetCard.SkillApplyInformation.AttachedSkillsInfo.CreatorSkillList.Count(); i++)
			{
				AttachedSkillsPublishCount.Add(NetworkBattleGenericTool.GetPublishSkillCount(_targetCard.SkillApplyInformation.AttachedSkillsInfo.CreatorSkillList.ElementAt(i)));
			}
		}
		if (skill.SkillPrm.ownerCard.Skills.Any((SkillBase s) => s.IsRefVariable(SkillFilterCreator.ContentKeyword.cost.ToString()) && !s.IsRefVariable(SkillFilterCreator.ContentKeyword.base_cost.ToString()) && s.IsRefVariable(SkillFilterCreator.ContentKeyword.last_target.ToString())))
		{
			Cost = targetCard.Cost;
		}
		if (RegisterTool.HasTargetOverCostFromFilter(skill))
		{
			foreach (IReadOnlyBattleCardInfo item in (skill.ApplyCustomSelectFilterList.Find((ISkillCustomSelectFilter x) => x is SkillTargetOverCostFromLastTargetFilter) as SkillTargetOverCostFromLastTargetFilter).KeyDestroyedCard)
			{
				SkillKeyCardIdxList.Add(item.Index);
			}
		}
		if (skill.ApplyAndFilterIndexes.ContainsKey(targetCard))
		{
			RandomTargetIdx.AddRange(skill.ApplyAndFilterIndexes[targetCard]);
		}
		if (skill.IsRandomUntilDrawSkill)
		{
			IsShortageDeck = skill.SkillPrm.ownerCard.SelfBattlePlayer.IsShortageDeck || (skill as Skill_draw).IsActiveChangeShortageDeck;
		}
		IsInvoked = skill.IsInvoked;
	}

	public static void Event_SetApplyAndFilterIndex(SkillBase skill, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption)
	{
		skill.ApplyAndFilterIndexes.Clear();
		for (int i = 0; i < cards.Count(); i++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(i);
			for (int j = 0; j < skill.ApplyAndFilter.Count; j++)
			{
				if (IsMatchTargetToFilter(skill.ApplyAndFilter[j], battleCardBase, skill))
				{
					if (skill.ApplyAndFilterIndexes.ContainsKey(battleCardBase))
					{
						skill.ApplyAndFilterIndexes[battleCardBase].Add(j);
						continue;
					}
					skill.ApplyAndFilterIndexes.Add(battleCardBase, new List<int> { j });
				}
			}
		}
	}

	private static bool IsMatchTargetToFilter(ApplySkillTargetFilterCollection applyAndFilter, BattleCardBase targetCard, SkillBase skill)
	{
		List<IReadOnlyBattleCardInfo> cards = new List<IReadOnlyBattleCardInfo> { targetCard };
		foreach (ISkillCardFilter cardFilter in applyAndFilter.CardFilterList)
		{
			if (!cardFilter.Filtering(cards, skill.OptionValue).Any())
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsEventSettingSkillLastTarget(SkillBase skill)
	{
		if (!(skill is Skill_powerup))
		{
			return false;
		}
		return skill.ApplyingTargetFilter is SkillTargetLastTargetFilter;
	}

	public void RemoveIndexList(int index)
	{
		IndexList.Remove(index);
	}

	public bool IsSelfDiscard()
	{
		if (_targetCard.SelfBattlePlayer.SelfDiscardList.Any((BattleCardBase s) => s == _targetCard))
		{
			return true;
		}
		return false;
	}
}
