using System.Collections.Generic;
using Wizard.Battle;

public class AttachedSkillInformation
{
	public SkillCollectionBase AttachedSkills { get; protected set; }

	public List<string> OwnerCardNameList { get; protected set; }

	public List<int> OwnerCardIdList { get; protected set; }

	public List<long> DuplicateBanNum { get; protected set; }

	public List<SkillBase> CreatorSkillList { get; protected set; }

	public List<int> CreatorSkillIndexList { get; protected set; }

	public AttachedSkillInformation(BattleCardBase card)
	{
		AttachedSkills = new SkillCollectionBase(card);
		OwnerCardNameList = new List<string>();
		OwnerCardIdList = new List<int>();
		DuplicateBanNum = new List<long>();
		CreatorSkillList = new List<SkillBase>();
		CreatorSkillIndexList = new List<int>();
	}

	public AttachedSkillInformation(BattleCardBase card, SkillCollectionBase skills, List<string> nameList, List<int> idList, List<long> duplicateBanNum, List<SkillBase> createrList, List<int> creatorSkillIndexList)
	{
		AttachedSkills = skills.Clone(card);
		OwnerCardNameList = new List<string>(nameList);
		OwnerCardIdList = new List<int>(idList);
		DuplicateBanNum = new List<long>(duplicateBanNum);
		CreatorSkillList = new List<SkillBase>(createrList);
		CreatorSkillIndexList = new List<int>(creatorSkillIndexList);
	}

	public void Add(SkillBase skill, string ownerCardName, int ownerCardID, long duplicateBanNum, SkillBase creatorSkill, int index)
	{
		AttachedSkills.Add(skill);
		OwnerCardNameList.Add(ownerCardName);
		OwnerCardIdList.Add(ownerCardID);
		DuplicateBanNum.Add(duplicateBanNum);
		CreatorSkillList.Add(creatorSkill);
		CreatorSkillIndexList.Add(index);
	}

	public void Remove(SkillBase skill, BattleCardBase owner, long duplicateBanNum, SkillBase creatorSkill, int index)
	{
		string name = owner.GetName();
		int cardId = owner.CardId;
		Remove(skill, name, cardId, duplicateBanNum, creatorSkill, index);
	}

	public void Remove(SkillBase skill, string ownerCardName, int ownerCardID, long duplicateBanNum, SkillBase creatorSkill, int index)
	{
		AttachedSkills.Remove(skill);
		OwnerCardNameList.Remove(ownerCardName);
		OwnerCardIdList.Remove(ownerCardID);
		DuplicateBanNum.Remove(duplicateBanNum);
		CreatorSkillList.Remove(creatorSkill);
		CreatorSkillIndexList.Remove(index);
	}

	public void Clear()
	{
		AttachedSkills.Clear();
		OwnerCardNameList.Clear();
		OwnerCardIdList.Clear();
		DuplicateBanNum.Clear();
		CreatorSkillList.Clear();
		CreatorSkillIndexList.Clear();
	}
}
