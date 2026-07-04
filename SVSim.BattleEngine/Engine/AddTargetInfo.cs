using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;

public class AddTargetInfo
{
	private BattleCardBase _ownerCard;

	private ConditionSkillFilterCollection _conditionFilter;

	private ApplySkillTargetFilterCollection _targetFilter;

	private Func<SkillBase, bool> typeCheck;

	private string _conditionFilterText;

	private string _targetFilterText;

	private string _skillTypeText;

	private string _ownerCardtype;

	private SkillCreator _skillCreator;

	public AddTargetInfo(BattleCardBase ownerCard, string conditionFilterText, string targetFilterText, string skillTypeText, string ownerCardType, SkillBase skill)
	{
		_ownerCard = ownerCard;
		_conditionFilterText = conditionFilterText;
		_targetFilterText = targetFilterText;
		_skillTypeText = skillTypeText;
		_ownerCardtype = ownerCardType;
		_conditionFilter = new ConditionSkillFilterCollection();
		_targetFilter = new ApplySkillTargetFilterCollection();
		typeCheck = SetTypeCheck(_skillTypeText, _ownerCardtype);
		_skillCreator = _ownerCard.CreateSkillCreator(_ownerCard.SelfBattlePlayer, _ownerCard.OpponentBattlePlayer, _ownerCard.ResourceMgr);
		string[] array = _conditionFilterText.Split('&');
		List<SkillFilterCreator.ContentInfo> list = new List<SkillFilterCreator.ContentInfo>();
		for (int i = 0; i < array.Length; i++)
		{
			SkillFilterCreator.ParseContentInfo(array[i], out var retParsedInfo);
			list.Add(retParsedInfo);
		}
		SkillCreator.SetupSkillConditionOld(_conditionFilter, list, _ownerCard, skill);
		string[] array2 = _targetFilterText.Split('&');
		List<SkillFilterCreator.ContentInfo> list2 = new List<SkillFilterCreator.ContentInfo>();
		for (int j = 0; j < array2.Length; j++)
		{
			SkillFilterCreator.ParseContentInfo(array2[j], out var retParsedInfo2);
			list2.Add(retParsedInfo2);
		}
		_skillCreator.SetupSkillTargetOld(_targetFilter, _ownerCard, list2, skill);
	}

	private Func<SkillBase, bool> SetTypeCheck(string skillType, string ownerCardType)
	{
		if (skillType != null && skillType == "damage")
		{
			return (SkillBase skill) => CardTypeCheck(skill.SkillPrm.ownerCard, ownerCardType) && skill is Skill_damage;
		}
		return (SkillBase skill) => CardTypeCheck(skill.SkillPrm.ownerCard, ownerCardType) && skill is Skill_none;
	}

	private bool CardTypeCheck(BattleCardBase card, string ownerCardType)
	{
		return ownerCardType switch
		{
			"all" => true, 
			"unit" => card.IsUnit, 
			"spell" => card.IsSpell, 
			"field" => card.IsField, 
			"chant_field" => card.IsChantField, 
			_ => false, 
		};
	}

	public AddTargetInfo Clone(BattleCardBase ownerCard)
	{
		return new AddTargetInfo(ownerCard, _conditionFilterText, _targetFilterText, _skillTypeText, _ownerCardtype, null);
	}
}
