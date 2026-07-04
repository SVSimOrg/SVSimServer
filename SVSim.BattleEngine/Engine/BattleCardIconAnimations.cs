using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;

public class BattleCardIconAnimations : MonoBehaviour
{
	public class SkillIcon
	{
		public string _key;

		public string _iconSpriteName;

		public int LabelNumber;

		public SkillIcon(string key, string iconSpriteName, int labelNumber)
		{
			_key = key;
			_iconSpriteName = iconSpriteName;
			LabelNumber = labelNumber;
		}
	}

	private List<SkillIcon> skillIconList = new List<SkillIcon>();

	private List<SkillIcon> skillIconListWithoutDuplicates = new List<SkillIcon>();

	private CardTemplate cardTemplate;

	private BattleCardBase _card;

	private SkillCollectionBase collection;

	private bool skillIconAlphaFlg;

	private int skillCount;

	private int _inductionLabelNumber = -1;

	public VfxBase Initialize(BattleCardBase card, SkillCollectionBase collection, bool isStackWhiteRitual = false)
	{
		_card = card;
		this.collection = collection;
		ISkillApplyInformation skillApplyInformation = card.SkillApplyInformation;
		bool isEarthRiteField = (IsEarthRiteField() ? true : false);
		bool hasInductionSkill = HasInductionSkill();
		bool hasInductionNumberSkill = HasInductionNumberSkill();
		bool hasKiller = (skillApplyInformation.IsKiller ? true : false);
		bool hasDrain = (skillApplyInformation.IsDrain ? true : false);
		bool hasWhenDestroySkill = (HasWhenDestroySkill() ? true : false);
		bool hasGetonSkill = HasGetonSkill();
		bool isGetOnAfter = _card.GetOnCards.Any();
		bool hasWhiteRirualStackSkill = HasStackWhiteRitualSkill();
		int whiteRitualCount = _card.SkillApplyInformation.WhiteRitualCount;
		return InstantVfx.Create(delegate
		{
			InitializeIcon(isEarthRiteField && !hasWhiteRirualStackSkill, isEarthRiteField && hasWhiteRirualStackSkill, whiteRitualCount, hasInductionSkill, hasInductionNumberSkill, hasKiller, hasDrain, hasWhenDestroySkill, hasGetonSkill, isGetOnAfter, isReplay: false, isStackWhiteRitual);
		});
	}

	private void InitializeIcon(bool hasWhiteRirualSkill, bool hasWhiteRirualStackSkill, int whiteRitualCount, bool hasInductionSkill, bool hasInductionNumberSkill, bool hasKiller, bool hasDrain, bool hasWhenDestroySkill, bool hasGetonSkill, bool isGetOnAfter = false, bool isReplay = false, bool isStackWhiteRitual = false)
	{
		if (!(_card.BattleCardView.GameObject == null))
		{
			ClearAllSkillIcons();
			cardTemplate = _card.BattleCardView.GameObject.GetComponent<CardTemplate>();
			cardTemplate.SkillIconTemp.gameObject.transform.localPosition = new Vector3(0f, -30f, -0.1f);
			cardTemplate.SkillIconTemp.gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
			cardTemplate.SkillIconTemp.atlas = UIManager.GetInstance().GetAtlasList().FirstOrDefault((UIAtlas s) => s.name == "Battle");
			AddToIconList("white_ritual", "battle_notice_status_08", hasWhiteRirualSkill);
			AddToIconList("stack_white_ritual", "battle_notice_status_11", hasWhiteRirualStackSkill, whiteRitualCount);
			AddToIconList("induction", "battle_notice_status_04", hasInductionSkill);
			AddToIconList("induction_number", "battle_notice_status_04", hasInductionNumberSkill, GetInductionLabelNumber());
			AddToIconList("killer", "battle_notice_status_01", hasKiller);
			AddToIconList("drain", "battle_notice_status_07", hasDrain);
			AddToIconList("destroy", "battle_notice_status_06", hasWhenDestroySkill);
			if (isReplay)
			{
				AddToIconList("geton", "battle_notice_status_09", hasGetonSkill);
				AddToIconList("geton_after", "battle_notice_status_10", isGetOnAfter);
			}
			else if (isGetOnAfter)
			{
				AddToIconList("geton_after", "battle_notice_status_10", hasGetonSkill);
			}
			else
			{
				AddToIconList("geton", "battle_notice_status_09", hasGetonSkill);
			}
			PopulateSkillIconListWithoutDuplicates();
			string spriteName = (skillIconListWithoutDuplicates.Any() ? skillIconListWithoutDuplicates[0]._iconSpriteName : string.Empty);
			cardTemplate.SkillIconTemp.spriteName = spriteName;
			ChangeSkillIconLabel(cardTemplate.SkillIconLabelTemp, skillIconListWithoutDuplicates.Any() ? skillIconListWithoutDuplicates[0].LabelNumber : (-1));
			UpdateSkillIconLabelColor();
			skillCount = 0;
			cardTemplate.SkillIconTemp.gameObject.SetActive(value: true);
			if (isStackWhiteRitual)
			{
				cardTemplate.SkillIconTemp.alpha = 1.5f;
				skillIconAlphaFlg = false;
			}
		}
	}

	private void AddToIconList(string key, string spriteName, bool addCondition, int labelNumber = -1)
	{
		if (addCondition)
		{
			AddSkillIcon(key, spriteName, labelNumber);
		}
	}

	private void PopulateSkillIconListWithoutDuplicates()
	{
		AddToIconListWithoutDuplicates("white_ritual");
		AddToIconListWithoutDuplicates("stack_white_ritual");
		AddToIconListWithoutDuplicates("induction");
		AddToIconListWithoutDuplicates("induction_number");
		AddToIconListWithoutDuplicates("destroy");
		AddToIconListWithoutDuplicates("killer");
		AddToIconListWithoutDuplicates("drain");
		AddToIconListWithoutDuplicates("geton");
	}

	private void AddToIconListWithoutDuplicates(string key)
	{
		if (skillIconList.Any((SkillIcon c) => c._key == key) && !skillIconListWithoutDuplicates.Any((SkillIcon c) => c._key == key))
		{
			SkillIcon skillIcon = skillIconList.SingleOrDefault((SkillIcon c) => c._key == key && c._iconSpriteName != null);
			skillIconListWithoutDuplicates.Add(new SkillIcon(key, skillIcon._iconSpriteName, skillIcon.LabelNumber));
		}
	}

	public void AddSkillIcon(string key, string fileName, int labelNumber = -1)
	{
		string iconSpriteName = ((!skillIconList.Any((SkillIcon v) => v._key == key)) ? fileName : null);
		skillIconList.Add(new SkillIcon(key, iconSpriteName, labelNumber));
		skillIconListWithoutDuplicates = skillIconList.Where((SkillIcon v) => v._iconSpriteName != null).ToList();
	}

	public void DeleteSkillIcon(string key)
	{
		if (skillIconList.Any((SkillIcon v) => v._key == key))
		{
			skillIconList.Remove(skillIconList.Where((SkillIcon v) => v._key == key).Last());
		}
		skillIconListWithoutDuplicates = skillIconList.Where((SkillIcon v) => v._iconSpriteName != null).ToList();
		if (skillIconListWithoutDuplicates.Count == 0)
		{
			cardTemplate.SkillIconTemp.spriteName = string.Empty;
			cardTemplate.SkillIconLabelTemp.text = string.Empty;
		}
		ChangeTexture();
	}

	private void ChangeTexture()
	{
		if (skillIconListWithoutDuplicates.Count() - 1 > skillCount)
		{
			skillCount++;
			cardTemplate.SkillIconTemp.spriteName = skillIconListWithoutDuplicates[skillCount]._iconSpriteName;
			ChangeSkillIconLabel(cardTemplate.SkillIconLabelTemp, skillIconListWithoutDuplicates[skillCount].LabelNumber);
		}
		else if (skillIconListWithoutDuplicates.Count() != 0)
		{
			skillCount = 0;
			cardTemplate.SkillIconTemp.spriteName = skillIconListWithoutDuplicates[skillCount]._iconSpriteName;
			ChangeSkillIconLabel(cardTemplate.SkillIconLabelTemp, skillIconListWithoutDuplicates[skillCount].LabelNumber);
		}
		UpdateSkillIconLabelColor();
	}

	private void UpdateSkillIconLabelColor()
	{
		if (cardTemplate.SkillIconTemp.spriteName == "battle_notice_status_11")
		{
			cardTemplate.SkillIconLabelTemp.color = Color.white;
			cardTemplate.SkillIconLabelTemp.effectColor = Color.black;
		}
		else if (cardTemplate.SkillIconTemp.spriteName == "battle_notice_status_04")
		{
			cardTemplate.SkillIconLabelTemp.color = Color.black;
			cardTemplate.SkillIconLabelTemp.effectColor = Color.white;
		}
	}

	private void ChangeSkillIconLabel(UILabel label, int labelNumber)
	{
		if (labelNumber == -1)
		{
			label.text = string.Empty;
		}
		else
		{
			label.text = labelNumber.ToString();
		}
	}

	public void ClearAllSkillIcons()
	{
		skillIconList.Clear();
		skillIconListWithoutDuplicates.Clear();
	}

	private bool HasWhenDestroySkill()
	{
		return collection._skillTimingInfo.IsWhenDestroy;
	}

	public bool HasInductionSkill()
	{
		for (int i = 0; i < collection.Count(); i++)
		{
			SkillBase skillBase = collection.ElementAt(i);
			if (skillBase.IsInductionSkill && skillBase.SkillPrm.buildInfo._icon == "induction")
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStackWhiteRitualSkill()
	{
		return collection.Any((SkillBase x) => x is Skill_stack_white_ritual);
	}

	public bool HasGetonSkill()
	{
		return collection.Any((SkillBase x) => x is Skill_geton);
	}

	public bool HasInductionNumberSkill()
	{
		for (int i = 0; i < collection.Count(); i++)
		{
			SkillBase skillBase = collection.ElementAt(i);
			if (skillBase.IsInductionSkill && skillBase.SkillPrm.buildInfo._icon != "induction" && skillBase.SkillPrm.buildInfo._icon.Contains("induction"))
			{
				return true;
			}
		}
		return false;
	}

	public int GetInductionLabelNumber()
	{
		if (_inductionLabelNumber != -1)
		{
			return _inductionLabelNumber;
		}
		SkillBase skillBase = collection.FirstOrDefault((SkillBase s) => s.IsInductionSkill && s.SkillPrm.buildInfo._icon != "induction" && s.SkillPrm.buildInfo._icon.Contains("induction"));
		if (skillBase == null)
		{
			return -1;
		}
		SkillOptionValue skillOptionValue = new SkillOptionValue(skillBase.SkillPrm.buildInfo._icon);
		skillOptionValue.SetupFilterVariable(_card.SelfBattlePlayer.BattleMgr.GetBattlePlayerInfoPair(_card.IsPlayer), _card, isPrePlay: false, null);
		return skillOptionValue.GetInt(SkillFilterCreator.ContentKeyword.induction);
	}

	private bool IsEarthRiteField()
	{
		if (_card.IsField || _card.IsChantField)
		{
			return _card.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL);
		}
		return false;
	}

	public VfxBase UpdateSkillIconInReplay(List<NetworkBattleReceiver.InplaySkillEffect> inplaySkillEffectList, int inductionNumber, bool isInitialize, bool isStackWhiteRitual = false)
	{
		if (!isInitialize && _card.HasStackWhiteRitualAndOtherIconSkill() && skillIconListWithoutDuplicates.Count < 2)
		{
			return NullVfx.GetInstance();
		}
		_inductionLabelNumber = inductionNumber;
		bool hasWhiteRitualSkill = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.WhiteRitual);
		bool hasWhiteRirualStackSkill = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.StackWhiteRitual);
		bool hasInductionSkill = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Induction);
		bool hasInductionNumberSkill = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.InductionNumber);
		bool hasKiller = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Killer);
		bool hasDrain = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Drain);
		bool hasWhenDestroySkill = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Destroy);
		bool hasGeton = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Geton);
		bool hasGetonAfter = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.GetonAfter);
		int whiteRitualCount = _card.SkillApplyInformation.WhiteRitualCount;
		return InstantVfx.Create(delegate
		{
			if (skillIconList.Count == 0 || isInitialize)
			{
				InitializeIcon(hasWhiteRitualSkill, hasWhiteRirualStackSkill, whiteRitualCount, hasInductionSkill, hasInductionNumberSkill, hasKiller, hasDrain, hasWhenDestroySkill, hasGeton, hasGetonAfter, isReplay: true, isStackWhiteRitual);
			}
			else
			{
				UpdateSkillIcon("white_ritual", "battle_notice_status_08", hasWhiteRitualSkill);
				UpdateSkillIcon("stack_white_ritual", "battle_notice_status_11", hasWhiteRirualStackSkill, whiteRitualCount);
				UpdateSkillIcon("induction", "battle_notice_status_04", hasInductionSkill);
				UpdateSkillIcon("induction_number", "battle_notice_status_04", hasInductionNumberSkill, GetInductionLabelNumber());
				UpdateSkillIcon("killer", "battle_notice_status_01", hasKiller);
				UpdateSkillIcon("drain", "battle_notice_status_07", hasDrain);
				UpdateSkillIcon("destroy", "battle_notice_status_06", hasWhenDestroySkill);
				UpdateSkillIcon("geton", "battle_notice_status_09", hasGeton);
				UpdateSkillIcon("geton_after", "battle_notice_status_10", hasGetonAfter);
			}
		});
	}

	private void UpdateSkillIcon(string key, string spriteName, bool hasIcon, int labelNumber = -1)
	{
		if (hasIcon && !skillIconList.Any((SkillIcon v) => v._key == key))
		{
			AddToIconList(key, spriteName, hasIcon, labelNumber);
		}
		else if (!hasIcon && skillIconList.Any((SkillIcon v) => v._key == key))
		{
			DeleteSkillIcon(key);
		}
	}
}
