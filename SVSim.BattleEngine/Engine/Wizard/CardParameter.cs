using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using Wizard.Battle.View;
// TODO(engine-cleanup-pass2): 70 of 178 methods unrun in baseline
//   Type: Wizard.CardParameter
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class CardParameter
{
	public class AttackEffectParameter
	{

		private List<string> _effectPath;

		private List<string> _se;

		private List<EffectMgr.MoveType> _moveType;

		private List<EffectMgr.EngineType> _effectEnginType;

		private List<float> _time;

		public AttackEffectParameter(CardCSVData card)
		{
			string normal = "";
			string evolve = "";
			SpritParameter(card.atk_effect_path, ref normal, ref evolve);
			_effectPath = new List<string>();
			_effectPath.Add(normal);
			_effectPath.Add(evolve);
			SpritParameter(card.atk_se, ref normal, ref evolve);
			_se = new List<string>();
			_se.Add(normal);
			_se.Add(evolve);
			SpritParameter(card.atk_move_type, ref normal, ref evolve);
			_moveType = new List<EffectMgr.MoveType>();
			_moveType.Add(EffectMgr.ToStrMoveType(normal));
			_moveType.Add(EffectMgr.ToStrMoveType(evolve));
			SpritParameter(card.atk_effect_engin_type, ref normal, ref evolve);
			_effectEnginType = new List<EffectMgr.EngineType>();
			_effectEnginType.Add(EffectMgr.ToStrEngineType(normal));
			_effectEnginType.Add(EffectMgr.ToStrEngineType(evolve));
			SpritParameter(card.atk_time, ref normal, ref evolve);
			_time = new List<float>();
			_time.Add(float.Parse(normal));
			_time.Add(float.Parse(evolve));
		}

		public void SpritParameter(string parameter, ref string normal, ref string evolve)
		{
			List<string> list = parameter.Split(new string[1] { "//" }, StringSplitOptions.None).ToList();
			if (list.Count > 1)
			{
				normal = list[0];
				evolve = list[1];
			}
			else if (list.Count == 1 && list[0] != "")
			{
				normal = list[0];
				evolve = list[0];
			}
			else
			{
				normal = "";
				evolve = "";
			}
		}

		public string GetEffectPath(bool isEvolve)
		{
			int index = (isEvolve ? 1 : 0);
			return _effectPath[index];
		}
	}

	private static readonly Vector4 DEFAULT_TEXTURE_TILLILNG_OFFSET = new Vector4(1f, 1f, 0f, 0f);

	private string _cardNameId = string.Empty;

	private string _cardName;

	private string _tribeNameId = string.Empty;

	private string _tribeName;

	private string _skillDescriptionId = string.Empty;

	private string _skillDescription;

	private string _evoSkillDescriptionId = string.Empty;

	private string _evoSkillDescription;

	private string _descriptionId = string.Empty;

	private string _evoDescriptionId = string.Empty;

	private string _cardVoiceId = string.Empty;

	private string _cardVoice;

	private string _convertedSkillDescription;

	private string _convertedEvoSkillDescription;

	public string CardName
	{
		get
		{
			if (_cardName != null)
			{
				return _cardName;
			}
			return _cardName = ConvCardName(_cardNameId);
		}
	}

	public string CardHiragana => ConvCardNameToHiragana(CardName);

	public string TribeName
	{
		get
		{
			if (_tribeName != null)
			{
				return _tribeName;
			}
			return _tribeName = ConvTribeName(_tribeNameId);
		}
	}

	public bool IsTribeAll => _tribeNameId == "TN_すべて";

	public string SkillDescription
	{
		get
		{
			if (_skillDescription != null)
			{
				return _skillDescription;
			}
			return _skillDescription = ConvSkillDescription(_skillDescriptionId);
		}
		set
		{
			_skillDescription = value;
		}
	}

	public string EvoSkillDescription
	{
		get
		{
			if (_evoSkillDescription != null)
			{
				return _evoSkillDescription;
			}
			return _evoSkillDescription = ConvSkillDescription(_evoSkillDescriptionId);
		}
		set
		{
			_evoSkillDescription = value;
		}
	}

	public string CardVoice
	{
		get
		{
			if (_cardVoice != null)
			{
				return _cardVoice;
			}
			return _cardVoice = ConvCardVoiceText(_cardVoiceId);
		}
	}

	public string ConvertedSkillDescription
	{
		get
		{
			if (_convertedSkillDescription != null)
			{
				return _convertedSkillDescription;
			}
			_convertedSkillDescription = BattleCardBase.ConvertSkillDescriptionText(SkillDescription);
			if (_convertedSkillDescription == null)
			{
				_convertedSkillDescription = string.Empty;
			}
			return _convertedSkillDescription;
		}
	}

	public string ConvertedEvoSkillDescription
	{
		get
		{
			if (_convertedEvoSkillDescription != null)
			{
				return _convertedEvoSkillDescription;
			}
			_convertedEvoSkillDescription = BattleCardBase.ConvertSkillDescriptionText(EvoSkillDescription);
			if (_convertedEvoSkillDescription == null)
			{
				_convertedEvoSkillDescription = string.Empty;
			}
			return _convertedEvoSkillDescription;
		}
	}

	public int CardId { get; private set; }

	public bool IsChoiceEvolutionCard => CardId / 1000000 == 910;

	public string CardHashId { get; private set; }

	public int ResourceCardId { get; private set; }

	public string CardSetId { get; private set; }

	public bool IsFoil { get; private set; }

	public string Path { get; private set; }

	public CardBasePrm.CharaType CharType { get; private set; }

	public CardBasePrm.ClanType Clan { get; private set; }

	public List<CardBasePrm.TribeType> Tribe { get; private set; }

	public string Skill { get; private set; }

	public string SkillTiming { get; private set; }

	public string SkillCondition { get; private set; }

	public string SkillTarget { get; private set; }

	public string SkillOption { get; private set; }

	public string SkillPreprocess { get; private set; }

	public HandCardFrameEffectType[] SkillHandCardFrameEffectType { get; private set; }

	public string SkillIcon { get; private set; }

	public string SummonEffectPath { get; private set; }

	public string SummonSePath { get; private set; }

	public EffectMgr.MoveType SummonMoveType { get; private set; }

	public EffectMgr.EngineType SummonEffectType { get; private set; }

	public float SummonTime { get; private set; }

	public AttackEffectParameter AtkEffectParameter { get; private set; }

	public string[] SkillEffectPath { get; private set; }

	public string[] SkillSe { get; private set; }

	public EffectMgr.MoveType[] SkillMoveType { get; private set; }

	public EffectMgr.EngineType[] SkillEffectEnginType { get; private set; }

	public string[] SkillEffectTime { get; private set; }

	public EffectMgr.TargetType[] SkillEffectTargetType { get; private set; }

	public EffectMgr.TargetType[] EvoSkillEffectTargetType { get; private set; }

	public string[] EvoSkillEffectPath { get; private set; }

	public string[] EvoSkillSe { get; private set; }

	public EffectMgr.MoveType[] EvoSkillMoveType { get; private set; }

	public EffectMgr.EngineType[] EvoSkillEffectEnginType { get; private set; }

	public string[] EvoSkillEffectTime { get; private set; }

	public bool IsResurgentCard { get; private set; }

	public int Cost { get; private set; }

	public bool IsVariableCost { get; private set; }

	public int Atk { get; private set; }

	public int Life { get; private set; }

	public int EvoAtk { get; private set; }

	public int EvoLife { get; private set; }

	public int ChantCount { get; private set; }

	public int Rarity { get; private set; }

	public int GetRedEther { get; private set; }

	public int UseRedEther { get; private set; }

	public string[] EvolEffectPath { get; private set; }

	public string[] EvolSePath { get; private set; }

	public EffectMgr.EngineType EvoEffectType { get; private set; }

	public float EvolTime { get; private set; }

	public string DestroyEffectPath { get; private set; }

	public int BaseCardId { get; private set; }

	public int NormalCardId { get; private set; }

	public int FoilCardId { get; private set; }

	public string PlayVoice { get; private set; }

	public string EvoVoice { get; private set; }

	public string AtkVoice { get; private set; }

	public string DestroyVoice { get; private set; }

	public string SkillVoice { get; private set; }

	public Vector2 NormalTilling { get; private set; }

	public Vector2 NormalOffset { get; private set; }

	public Vector2 EvolTilling { get; private set; }

	public Vector2 EvolOffset { get; private set; }

	public bool IsPrizeCard => CardSetNameMgr.IsPrizeSetId(int.Parse(CardSetId));

	public bool IsBasicCard => CardSetNameMgr.IsBasicSetId(int.Parse(CardSetId));

	public bool IsTokenCard => CardSetNameMgr.IsTokenSetId(int.Parse(CardSetId));

	public bool IsCollaboCard => CardSetNameMgr.IsCollaboSetId(int.Parse(CardSetId));

	public bool IsPhantomCard => SealedData.IsPhantomCard(CardId);

	public bool IsReprintedCard => CardId / 100000000 == 7;

	public bool IsPreReleaseCard
	{
		get
		{
			if (Prerelease.Status == Prerelease.eStatus.PRE_ROTATION && CardSetId == Prerelease.Instance.NextCardSetId.ToString())
			{
				return true;
			}
			return false;
		}
	}

	public bool IsNotCraftDestruct
	{
		get
		{
			if (GetRedEther > 0 || UseRedEther > 0)
			{
				return IsPreReleaseCard;
			}
			return true;
		}
	}

	public bool CanCraft
	{
		get
		{
			if (IsPreReleaseCard)
			{
				return false;
			}
			return UseRedEther > 0;
		}
	}

	public int SameKindNumMaxInUnlimited { get; private set; }

	public int SameKindNumMaxInCrossoverMainClass { get; private set; }

	public int SameKindNumMaxInCrossoverSubClass { get; private set; }

	public int SortIndex { get; private set; }

	public int GetSameKindNumMaxInFormat(Format inFormat, IFormatBehavior behavior, ClassType classType, MyRotationInfo myRotationInfo = null)
	{
		int result = 0;
		if (IsAvailableFormat(inFormat, classType, myRotationInfo))
		{
			switch (inFormat)
			{
			case Format.Unlimited:
				result = SameKindNumMaxInUnlimited;
				if (IsResurgentCard)
				{
					result = 0;
				}
				break;
			case Format.Crossover:
				result = ((classType == ClassType.MainClass) ? SameKindNumMaxInCrossoverMainClass : SameKindNumMaxInCrossoverSubClass);
				break;
			case Format.MyRotation:
				result = myRotationInfo.GetSameCardCount(BaseCardId);
				break;
			default:
				result = behavior.DeckSameKindCardNumMax;
				break;
			}
		}
		return result;
	}

	public bool IsAvailableFormat(Format inFormat, ClassType classType, MyRotationInfo myRotationInfo = null)
	{
		int num = int.Parse(CardSetId);
		Prerelease instance = Prerelease.Instance;
		switch (inFormat)
		{
		case Format.PreRotation:
			if (instance.RotationCardSetList.Contains(num) || instance.ReprintedBaseCardIds.Contains(BaseCardId))
			{
				return true;
			}
			return false;
		case Format.Rotation:
			if (Data.Load.data.RotationCardSetList.Contains(num) || Data.Load.data.ReprintedBaseCardIds.Contains(BaseCardId))
			{
				return true;
			}
			if (Prerelease.Status == Prerelease.eStatus.PRE_ROTATION && instance.NextCardSetId == num && instance.LatestReprintedBaseCardIds.Contains(BaseCardId))
			{
				return true;
			}
			return false;
		case Format.Unlimited:
			if (IsResurgentCard)
			{
				return false;
			}
			if (Prerelease.Status == Prerelease.eStatus.PRE_ROTATION && instance.NextCardSetId == num && !instance.ReprintedBaseCardIds.Contains(BaseCardId))
			{
				return false;
			}
			return SameKindNumMaxInUnlimited > 0;
		case Format.Sealed:
			return Data.ArenaData.SealedMyPageResponseData.CardPackIdList.Contains(CardSetId);
		case Format.Crossover:
			if (!Data.Crossover.CardSetIdList.Contains(num) && !Data.Crossover.ReprintedBaseCardIds.Contains(BaseCardId))
			{
				return false;
			}
			if (classType != ClassType.MainClass)
			{
				return SameKindNumMaxInCrossoverSubClass > 0;
			}
			return SameKindNumMaxInCrossoverMainClass > 0;
		case Format.MyRotation:
			if (myRotationInfo.GetSameCardCount(BaseCardId) == 0)
			{
				return false;
			}
			if (myRotationInfo.IsRePrintCard(BaseCardId))
			{
				return true;
			}
			if (myRotationInfo.IsEnableCardPackId(CardSetId))
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public CardParameter(CardBasePrm.ClanType clan = CardBasePrm.ClanType.ALL)
	{
		Life = 20;
		Clan = clan;
	}

	public void ChangeClanParameter(CardBasePrm.ClanType clan = CardBasePrm.ClanType.ALL)
	{
		Clan = clan;
	}

	public CardParameter(CardCSVData card)
	{
		SortIndex = card.SortIndex;
		CardId = int.Parse(card.card_id);
		if (string.IsNullOrEmpty(card.resource_card_id))
		{
			ResourceCardId = CardId;
		}
		else
		{
			ResourceCardId = int.Parse(card.resource_card_id);
		}
		if (!CardMaster.IsUseLocalCardMaster())
		{
			CardHashId = card.CardHashId;
		}
		CardSetId = card.card_set_id;
		IsFoil = card.is_foil == "1";
		Path = card.path;
		CharType = CardBasePrm.ToStrCharaType(card.char_type);
		Clan = CardBasePrm.ToStrClanType(card.clan);
		Tribe = CreateTribeList(card.tribe);
		Skill = card.skill;
		SkillTiming = card.skill_timing;
		SkillCondition = card.skill_condition;
		SkillTarget = card.skill_target;
		SkillOption = card.skill_option;
		SkillPreprocess = card.skill_preprocess;
		SkillHandCardFrameEffectType = HandCardFrameEffectControl.ToStrFrameEffect(card.skill_effect_condition);
		SkillIcon = card.skill_icon;
		SummonEffectPath = card.summon_effect_path;
		SummonSePath = card.summon_se_path;
		SummonMoveType = EffectMgr.ToStrMoveType(card.summon_move_type);
		SummonEffectType = EffectMgr.ToStrEngineType(card.summon_effect_type);
		SummonTime = ToFloat(card.summon_time);
		AtkEffectParameter = new AttackEffectParameter(card);
		SkillEffectPath = SplitString(card.skill_effect_path);
		SkillSe = SplitString(card.skill_se);
		SkillMoveType = Str2Enum(SplitString(card.skill_move_type).ToArray(), EffectMgr.ToStrMoveType);
		SkillEffectEnginType = Str2Enum(SplitString(card.skill_effect_engin_type), EffectMgr.ToStrEngineType);
		SkillEffectTime = SplitString(card.skill_effect_time);
		string[] array = SkillCreator.SplitBothSkillText(card.skill_effect_target_type);
		SkillEffectTargetType = Str2Enum(SplitString(array[0]), EffectMgr.ToStrTargetType);
		EvoSkillEffectTargetType = ((array.Length >= 2) ? Str2Enum(SplitString(array[1]), EffectMgr.ToStrTargetType) : SkillEffectTargetType);
		EvoSkillEffectPath = SplitString(card.evo_skill_effect_path);
		EvoSkillSe = SplitString(card.evo_skill_se);
		EvoSkillMoveType = Str2Enum(SplitString(card.evo_skill_move_type).ToArray(), EffectMgr.ToStrMoveType);
		EvoSkillEffectEnginType = Str2Enum(SplitString(card.evo_skill_effect_engin_type), EffectMgr.ToStrEngineType);
		EvoSkillEffectTime = SplitString(card.evo_skill_effect_time);
		Cost = int.Parse(card.cost);
		IsVariableCost = Cost == -99;
		if (IsVariableCost)
		{
			Cost = 0;
		}
		Atk = int.Parse(card.atk);
		Life = int.Parse(card.life);
		EvoAtk = int.Parse(card.evo_atk);
		EvoLife = int.Parse(card.evo_life);
		ChantCount = int.Parse(card.chant_count);
		Rarity = int.Parse(card.rarity);
		GetRedEther = int.Parse(card.get_red_ether);
		UseRedEther = int.Parse(card.use_red_ether);
		EvolEffectPath = SplitString(card.evol_effect_path);
		EvolSePath = SplitString(card.evol_se_path);
		EvoEffectType = EffectMgr.ToStrEngineType(card.evo_effect_type);
		EvolTime = ToFloat(card.evol_time);
		DestroyEffectPath = card.destroy_effect_path;
		PlayVoice = card.play_voice;
		EvoVoice = card.evo_voice;
		AtkVoice = card.atk_voice;
		DestroyVoice = card.destroy_voice;
		SkillVoice = card.skill_voice;
		BaseCardId = int.Parse(card.base_card_id);
		NormalCardId = int.Parse(card.normal_card_id);
		FoilCardId = int.Parse(card.foil_card_id);
		_cardNameId = card.CardNameId;
		_tribeNameId = card.TribeNameId;
		_descriptionId = card.DescriptionId;
		_evoDescriptionId = card.EvoDescriptionId;
		_cardVoiceId = card.CardVoiceId;
		IsResurgentCard = card.IsResurgentCard == "1";
		if (card.IsOverrideSkillDescription == "1")
		{
			_skillDescriptionId = $"OVR_{card.SkillDescriptionId}";
			_evoSkillDescriptionId = $"OVR_{card.EvoSkillDescriptionId}";
		}
		else
		{
			_skillDescriptionId = card.SkillDescriptionId;
			_evoSkillDescriptionId = card.EvoSkillDescriptionId;
		}
		Vector4 vector = ConvertToTextureTillingOffset(card.tilling_normal_x, card.tilling_normal_y, card.offset_normal_x, card.offset_normal_y);
		NormalTilling = new Vector2(vector.x, vector.y);
		NormalOffset = new Vector2(vector.z, vector.w);
		Vector4 vector2 = ConvertToTextureTillingOffset(card.tilling_evol_x, card.tilling_evol_y, card.offset_evol_x, card.offset_evol_y);
		EvolTilling = new Vector2(vector2.x, vector2.y);
		EvolOffset = new Vector2(vector2.z, vector2.w);
		SameKindNumMaxInUnlimited = FormatBehaviorManager.GetDefaultBehaviour(Format.Unlimited).DeckSameKindCardNumMax;
		for (int i = 0; i < Data.Load.data.UnlimitedRestrictedCardList.Count; i++)
		{
			UnlimitedRestrictedCard unlimitedRestrictedCard = Data.Load.data.UnlimitedRestrictedCardList[i];
			if (BaseCardId == unlimitedRestrictedCard.BaseCardId)
			{
				SameKindNumMaxInUnlimited = unlimitedRestrictedCard.Count;
				break;
			}
		}
		int deckSameKindCardNumMax = FormatBehaviorManager.GetDefaultBehaviour(Format.Crossover).DeckSameKindCardNumMax;
		CrossoverRestrictedCard restrictedCard = Data.Crossover.RestrictedCard;
		SameKindNumMaxInCrossoverMainClass = restrictedCard.GetRestrictedCountOrDefault(BaseCardId, ClassType.MainClass, deckSameKindCardNumMax);
		SameKindNumMaxInCrossoverSubClass = restrictedCard.GetRestrictedCountOrDefault(BaseCardId, ClassType.SubClass, deckSameKindCardNumMax);
	}

	public static List<CardBasePrm.TribeType> CreateTribeList(string tribeText)
	{
		List<CardBasePrm.TribeType> list = new List<CardBasePrm.TribeType>();
		string[] array = tribeText.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(CardBasePrm.ToStrTribeType(array[i]));
		}
		return list;
	}

	private string ConvCardName(string id)
	{
		return Data.Master.GetCardNameText(id);
	}

	private string ConvCardNameToHiragana(string cardName)
	{
		string text = "";
		int num = 0;
		bool flag = false;
		bool flag2 = true;
		while (num < cardName.Length)
		{
			if (cardName[num] == '[')
			{
				if (cardName.Substring(num, 5) == "[rub<")
				{
					num += 5;
					flag2 = false;
					flag = true;
				}
				else if (cardName.Substring(num, 6) == "[/rub]")
				{
					num += 6;
					flag2 = true;
				}
			}
			else if (cardName[num] == '>' && cardName.Substring(num, 2) == ">]")
			{
				num += 2;
				flag = false;
			}
			else
			{
				if (flag || flag2)
				{
					text += cardName[num];
				}
				num++;
			}
		}
		return ConvertToHiragana(text);
	}

	private string ConvertToHiragana(string s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = s.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (c >= 'ァ' && c <= 'ヴ')
			{
				c = (char)(c - 96);
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private string ConvTribeName(string id)
	{
		return Data.Master.GetTribeNameText(id);
	}

	private string ConvSkillDescription(string id)
	{
		return Data.Master.GetSkillDescText(id);
	}

	private string ConvCardVoiceText(string id)
	{
		return Data.Master.GetCardVoiceText(id);
	}

	public CardParameter Clone()
	{
		return (CardParameter)MemberwiseClone();
	}

	public CardParameter Clone(int cardId)
	{
		CardParameter cardParameter = Clone();
		cardParameter.CardId = cardId;
		return cardParameter;
	}

	private float ToFloat(string stringValue, float defaultValue = 0f)
	{
		float value = defaultValue;
		TryToFloat(stringValue, out value);
		return value;
	}

	private bool TryToFloat(string stringValue, out float value)
	{
		return float.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
	}

	private Vector4 ConvertToTextureTillingOffset(string stringTillingX, string stringTillingY, string stringOffsetX, string stringOffsetY)
	{
		float value = 0f;
		float value2 = 0f;
		float value3 = 0f;
		float value4 = 0f;
		if (TryToFloat(stringTillingX, out value) && TryToFloat(stringTillingY, out value2) && TryToFloat(stringOffsetX, out value3) && TryToFloat(stringOffsetY, out value4))
		{
			return new Vector4(value, value2, value3, value4);
		}
		return DEFAULT_TEXTURE_TILLILNG_OFFSET;
	}

	private static string[] SplitString(string source)
	{
		if (source == null)
		{
			return new string[0];
		}
		return source.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[1] { "" };
	}

	private T[] Str2Enum<T>(string[] str, Func<string, T> conv)
	{
		T[] array = new T[str.Length];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = conv(str[i]);
		}
		return array;
	}

	public void UpdateEvoAtkLife(int evoAtk, int evoLife)
	{
		EvoAtk = evoAtk;
		EvoLife = evoLife;
	}
}
