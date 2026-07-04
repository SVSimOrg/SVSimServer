using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;
using Wizard;

public class UIBase_CardManager : MonoBehaviour
{
	public enum LOAD_KIND
	{
	}

	public enum ScrollType
	{
	}

	private enum AddCardResult
	{
	}

	public class CardObjData
	{
		public GameObject CardObj;

		public string Cost;

		public int CostNum;

		public int mainCardNum;

		public int subCardNum;

		public int ids;

		public string lifes;

		public string Atks;

		public string Names;

		public string Skills;

		public string Evo_Costs;

		public string Evo_lifes;

		public string Evo_Atks;

		public string Evo_Names;

		public string Evo_Skills;

		public bool EvolOk;

		public CardBasePrm.ClanType clan;

		public List<CardBasePrm.TribeType> tribe;

		public CardBasePrm.CharaType cardType;

		public bool isPremiere;

		public int TotalCardNum => mainCardNum + subCardNum;
	}

	public class CardPrefabs
	{
	}

	public class LoadCondition
	{
		public Dictionary<int, int> CurrentIds;

		public int MaxCards;

		public int MaxSameCard;

		public LoadCondition(Dictionary<int, int> currentids, int maxCards, int maxSameCard)
		{
			CurrentIds = currentids;
			MaxCards = maxCards;
			MaxSameCard = maxSameCard;
		}

		public LoadCondition()
		{
			CurrentIds = new Dictionary<int, int>();
			MaxCards = 99999999;
			MaxSameCard = 9999999;
		}
	}

	public class ComparableCard : IComparable<ComparableCard>
	{
		private CardParameter prm;

		public ComparableCard(int id, CardMaster.CardMasterId cardMasterId)
		{
			prm = CardMaster.GetInstance(cardMasterId).GetCardParameterFromId(id);
		}

		public int CompareTo(ComparableCard other)
		{
			return prm.SortIndex.CompareTo(other.prm.SortIndex);
		}
	}

	public class FilterParameter
	{
		public enum LookBit
		{
			ALL,
			NORMAL,
			PREMIUM
		}

		public enum FavoriteBit
		{
			ALL,
			NORMAL,
			FAVORITE
		}

		public enum SpotBit
		{
			ALL,
			NORMAL,
			SPOT
		}

		public int Cost;

		public int Class;

		public int Foil;

		public int Type;

		public int Rarity;

		public int Favorite;

		public int Spot;

		public int Own;

		public int Craftable;

		public int Attack;

		public int Life;

		public string Word;

		public Format FormatState = Format.Max;

		public MyRotationInfo MyRotationInfoForFormatAvailable;

		public List<string> KeyWordList;

		public List<CardBasePrm.TribeType> TypeFilter;

		public List<string> CardSetIdList;

		public List<string> DisableCardSetidList = new List<string>();

		public List<string> CardSetIdList2 = new List<string>();

		public bool IsEnabledPrizeCard;

		public bool IsEnabledPhantomCard;

		public string CharacterVoice;

		public bool IsEnableResurgentCard = true;

		public ClassSet FixedClassSet;

		public FilterParameter(FilterParameter param)
		{
			Cost = param.Cost;
			Class = param.Class;
			Foil = param.Foil;
			Type = param.Type;
			Rarity = param.Rarity;
			Favorite = param.Favorite;
			Spot = param.Spot;
			Own = param.Own;
			Craftable = param.Craftable;
			Attack = param.Attack;
			Life = param.Life;
			Word = param.Word;
			FormatState = param.FormatState;
			KeyWordList = param.KeyWordList;
			TypeFilter = param.TypeFilter;
			CardSetIdList = param.CardSetIdList;
			DisableCardSetidList = param.DisableCardSetidList;
			CardSetIdList2 = param.CardSetIdList2;
			IsEnabledPrizeCard = param.IsEnabledPrizeCard;
			IsEnabledPhantomCard = param.IsEnabledPhantomCard;
			CharacterVoice = param.CharacterVoice;
			IsEnableResurgentCard = param.IsEnableResurgentCard;
			FixedClassSet = param.FixedClassSet;
		}

		public FilterParameter()
		{
			CardSetIdList = new List<string>();
		}

		public bool IsLookState(LookBit bit)
		{
			if (bit == LookBit.ALL)
			{
				if (Foil != 0)
				{
					if (IsLookState(LookBit.NORMAL))
					{
						return IsLookState(LookBit.PREMIUM);
					}
					return false;
				}
				return true;
			}
			return ((uint)Foil & (uint)bit) != 0;
		}

		public bool IsFavoriteState(FavoriteBit bit)
		{
			if (bit == FavoriteBit.ALL)
			{
				if (Favorite != 0)
				{
					if (IsFavoriteState(FavoriteBit.NORMAL))
					{
						return IsFavoriteState(FavoriteBit.FAVORITE);
					}
					return false;
				}
				return true;
			}
			return ((uint)Favorite & (uint)bit) != 0;
		}

		public bool IsSpotState(SpotBit bit)
		{
			if (bit == SpotBit.ALL)
			{
				if (Spot != 0)
				{
					if (IsSpotState(SpotBit.NORMAL))
					{
						return IsSpotState(SpotBit.SPOT);
					}
					return false;
				}
				return true;
			}
			return ((uint)Spot & (uint)bit) != 0;
		}

		public static bool operator ==(FilterParameter lhs, FilterParameter rhs)
		{
			FieldInfo[] fields = typeof(FilterParameter).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo obj in fields)
			{
				object value = obj.GetValue(lhs);
				object value2 = obj.GetValue(rhs);
				if ((value == null) ^ (value2 == null))
				{
					return false;
				}
				if (value != null && !value.Equals(value2))
				{
					return false;
				}
			}
			return true;
		}

		public static bool operator !=(FilterParameter lhs, FilterParameter rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private static readonly Color32 PREMIERE_GRADIENT_TOP_COLOR = new Color32(byte.MaxValue, 243, 176, byte.MaxValue);

	private static readonly Color32 PREMIERE_GRADIENT_BOTTOM_COLOR = new Color32(186, 150, 0, byte.MaxValue);

	private static readonly Color32 EFFECT_COLOR = new Color32(92, 56, 3, byte.MaxValue);

	[SerializeField]
	private UIFont _NormalCardFont;

	[SerializeField]
	private UIFont _PremiereCardFont;

	private long SleeveId = 3000011L;

	private CardKeyWordCommonCache _keyWordCommonCache;

	private CardKeyWordCache _keyWordCache;

	private CardKeyWordCache _cardNameKeyWordCache;

	private CardKeyWordCache _cardNameKeyWordHiraganaCache;

	public _3dCardFrameManager _3dCardFrameManager { get; private set; } = new _3dCardFrameManager();

	public List<string> AddAssetPath(List<int> List, bool is2D, CardMaster.CardMasterId cardMasterId, bool isAddSleeve = true, long sleeveId = 3000011L)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < List.Count; i++)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstance(cardMasterId).GetCardParameterFromId(List[i]);
			int resourceCardId = CardMaster.GetInstance(cardMasterId).GetCardParameterFromId(cardParameterFromId.NormalCardId).ResourceCardId;
			if (cardParameterFromId.CharType == CardBasePrm.CharaType.NORMAL)
			{
				string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(resourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial);
				if (!list.Contains(assetTypePath))
				{
					list.Add(assetTypePath);
				}
				continue;
			}
			string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(resourceCardId.ToString(), ResourcesManager.AssetLoadPathType.SpellCardMaterial);
			if (CardMaster.IsMutationCardCheck(cardParameterFromId.BaseCardId))
			{
				assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(resourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial);
			}
			if (!list.Contains(assetTypePath2))
			{
				list.Add(assetTypePath2);
			}
		}
		if (isAddSleeve)
		{
			SleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(sleeveId);
			string assetTypePath3 = Toolbox.ResourcesManager.GetAssetTypePath(SleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveMaterial);
			if (!list.Contains(assetTypePath3))
			{
				list.Add(assetTypePath3);
				list.Add(Toolbox.ResourcesManager.GetAssetTypePath(SleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture));
				if (Data.Master.SleeveMgr.Get(SleeveId).IsPremiumSleeve)
				{
					list.Add(Toolbox.ResourcesManager.GetAssetTypePath(SleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTextureMask));
				}
			}
		}
		return list;
	}

	public IList<int> SelectCardIDInConditionMask(List<int> idList, FilterParameter filterParam, IFormatBehavior formatBehaviour, MyRotationInfo myRotationInfo, bool alreadySorted = false, bool isCraftMode = false, bool isDisableAllTribe = false)
	{
		CardMaster cardMaster = CardMaster.GetInstance(formatBehaviour.CardMasterId);
		if (_keyWordCache == null)
		{
			_keyWordCache = new CardKeyWordCache();
		}
		if (_cardNameKeyWordCache == null)
		{
			_cardNameKeyWordCache = new CardKeyWordCache(CardKeyWordCache.Option.OnlyCardNames);
		}
		if (_cardNameKeyWordHiraganaCache == null)
		{
			_cardNameKeyWordHiraganaCache = new CardKeyWordCache(CardKeyWordCache.Option.OnlyCardNamesHiranaga);
		}
		Func<CardParameter, FilterParameter, bool> filters = null;
		if (formatBehaviour.IsConventionMode)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => formatBehaviour.GetPossessionCardNum(card.CardId, isIncludingSpotCard: true) > 0));
		}
		else
		{
			if (filterParam.Own != 0)
			{
				filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
				{
					int possessionCardNum = formatBehaviour.GetPossessionCardNum(card.CardId, isIncludingSpotCard: true);
					return (param.Own & ((possessionCardNum > 0) ? 1 : 2)) != 0;
				});
			}
			if (filterParam.Craftable != 0)
			{
				filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
				{
					int possessionCardNum = formatBehaviour.GetPossessionCardNum(card.CardId, isIncludingSpotCard: true);
					int num = ((card.UseRedEther > 0) ? 1 : 2);
					return possessionCardNum > 0 || (param.Craftable & num) != 0;
				});
			}
		}
		if (filterParam.Class != 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => (param.Class & (1 << (int)card.Clan)) != 0));
		}
		if (filterParam.Cost != 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => (param.Cost & (1 << Mathf.Min(card.Cost, 8))) != 0));
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.Attack != 0 && !CardBasePrm.IsFollowerCard(card.CharType))
			{
				return false;
			}
			return param.Attack == 0 || (param.Attack & (1 << Mathf.Min(card.Atk, 8))) != 0;
		});
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.Life != 0 && !CardBasePrm.IsFollowerCard(card.CharType))
			{
				return false;
			}
			return param.Life == 0 || (param.Life & (1 << Mathf.Min(card.Life, 8))) != 0;
		});
		if (filterParam.Foil != 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => (param.Foil & ((!card.IsFoil) ? 1 : 2)) != 0));
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.Type == 0)
			{
				return true;
			}
			int num = 0;
			switch (card.CharType)
			{
			case CardBasePrm.CharaType.NORMAL:
			case CardBasePrm.CharaType.EVOLUTION:
				num = 1;
				break;
			case CardBasePrm.CharaType.SPELL:
				num = 2;
				break;
			case CardBasePrm.CharaType.FIELD:
			case CardBasePrm.CharaType.CHANT_FIELD:
				num = 4;
				break;
			default:
				return false;
			}
			return (param.Type & num) != 0;
		});
		if (filterParam.Rarity != 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => (param.Rarity & (1 << card.Rarity - 1)) != 0));
		}
		if (filterParam.DisableCardSetidList.Count > 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => !param.DisableCardSetidList.Contains(card.CardSetId)));
		}
		if (filterParam.CardSetIdList2.Count > 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
			{
				if (param.FormatState == Format.MyRotation)
				{
					if (card.IsPrizeCard)
					{
						card = cardMaster.GetCardParameterFromId(card.BaseCardId);
					}
					if (myRotationInfo.IsNotUseCard(card.BaseCardId))
					{
						return false;
					}
					if (myRotationInfo.IsRePrintCard(card.BaseCardId))
					{
						foreach (string item in param.CardSetIdList2)
						{
							if (myRotationInfo.IsRePrintCardAvailablePack(card.BaseCardId, item))
							{
								return true;
							}
						}
					}
				}
				return param.CardSetIdList2.Contains(card.CardSetId) ? true : false;
			});
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.CardSetIdList.Count == 0 && !param.IsEnabledPrizeCard && !param.IsEnabledPhantomCard)
			{
				return true;
			}
			string cardSetId = card.CardSetId;
			if (card.IsCollaboCard)
			{
				cardSetId = CardMaster.GetInstance(formatBehaviour.CardMasterId).GetCardParameterFromId(card.BaseCardId).CardSetId;
			}
			if (param.CardSetIdList.Contains(cardSetId))
			{
				return true;
			}
			if (param.IsEnabledPrizeCard && card.IsPrizeCard)
			{
				return true;
			}
			return (param.IsEnabledPhantomCard && card.IsPhantomCard) ? true : false;
		});
		if (!filterParam.IsEnableResurgentCard)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => !card.IsResurgentCard));
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.IsFavoriteState(FilterParameter.FavoriteBit.ALL))
			{
				return true;
			}
			bool flag = false; // headless: no user favorites
			return (param.IsFavoriteState(FilterParameter.FavoriteBit.NORMAL) && !flag) || (param.IsFavoriteState(FilterParameter.FavoriteBit.FAVORITE) && flag);
		});
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.IsSpotState(FilterParameter.SpotBit.ALL) || (param.IsSpotState(FilterParameter.SpotBit.NORMAL) && param.IsSpotState(FilterParameter.SpotBit.SPOT)))
			{
				return true;
			}
			bool flag = false; // headless: no user spot cards
			if (isCraftMode)
			{
				if (param.IsSpotState(FilterParameter.SpotBit.SPOT))
				{
					return flag;
				}
				return true;
			}
			return param.IsSpotState(FilterParameter.SpotBit.SPOT) ? flag : false; // headless: no user card counts
		});
		if (filterParam.MyRotationInfoForFormatAvailable != null)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
			{
				if (card.IsPrizeCard)
				{
					card = cardMaster.GetCardParameterFromId(card.BaseCardId);
				}
				ClassType classType = ClassUtil.GetClassType(card, param.FormatState, param.FixedClassSet);
				return card.IsAvailableFormat(param.FormatState, classType, myRotationInfo);
			});
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.FormatState == Format.Unlimited || param.FormatState == Format.Rotation || param.FormatState == Format.PreRotation || param.FormatState == Format.Sealed || param.FormatState == Format.Crossover)
			{
				ClassType classType = ClassUtil.GetClassType(card, param.FormatState, param.FixedClassSet);
				return card.IsAvailableFormat(param.FormatState, classType, myRotationInfo);
			}
			return true;
		});
		if (filterParam.KeyWordList != null && filterParam.KeyWordList.Count > 0)
		{
			List<string> filterKeywordList = filterParam.KeyWordList;
			foreach (KeyValuePair<string, string> item2 in Data.Master.CardFilterKeywordReplaceDic)
			{
				if (filterKeywordList.Contains(item2.Value))
				{
					List<string> list = new List<string>(filterKeywordList.Count + 1);
					list.AddRange(filterKeywordList);
					list.Add(item2.Key);
					filterKeywordList = list;
				}
			}
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
			{
				IList<string> list2 = _keyWordCache.Get(card, _keyWordCommonCache);
				foreach (string item3 in filterKeywordList)
				{
					if (list2.Contains(item3))
					{
						return true;
					}
				}
				return false;
			});
		}
		if (filterParam.TypeFilter != null && filterParam.TypeFilter.Count > 0)
		{
			filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
			{
				if (isDisableAllTribe && card.IsTribeAll)
				{
					return false;
				}
				foreach (CardBasePrm.TribeType item4 in param.TypeFilter)
				{
					if (card.Tribe.Contains(item4))
					{
						return true;
					}
				}
				return false;
			});
		}
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)delegate(CardParameter card, FilterParameter param)
		{
			if (param.Word == null || param.Word.Length == 0)
			{
				return true;
			}
			List<string> list2 = new List<string> { CardBasePrm.GetCardTypeName(card.CharType) };
			if (!card.Tribe.Contains(CardBasePrm.TribeType.ALL))
			{
				if (card.IsTribeAll)
				{
					list2.AddRange(Data.Master.GetAllTribeNameList());
				}
				else
				{
					list2.Add(card.TribeName);
				}
			}
			list2.Add(Regex.Replace(card.ConvertedSkillDescription, "(\\[[a-zA-Z0-9\\/\\-]*(rub\\<[^\\>]*\\>)*\\])", ""));
			list2.Add(Regex.Replace(card.ConvertedEvoSkillDescription, "(\\[[a-zA-Z0-9\\/\\-]*(rub\\<[^\\>]*\\>)*\\])", ""));
			list2.Add(Regex.Replace(card.CardName, "(\\[[a-zA-Z0-9\\/\\-]*(rub\\<[^\\>]*\\>)*\\])", ""));
			string[] array = param.Word.Replace('\u3000', ' ').Split(' ');
			if (list2.Any((string src) => array.All((string req) => src.IndexOf(req, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				return true;
			}
			list2.Clear();
			CompareInfo ci = CultureInfo.CurrentCulture.CompareInfo;
			return list2.Any((string src) => array.All((string req) => ci.IndexOf(src, req, CompareOptions.IgnoreKanaType) >= 0));
		});
		filters = (Func<CardParameter, FilterParameter, bool>)Delegate.Combine(filters, (Func<CardParameter, FilterParameter, bool>)((CardParameter card, FilterParameter param) => string.IsNullOrEmpty(param.CharacterVoice) || card.CardVoice.IndexOf(param.CharacterVoice, StringComparison.OrdinalIgnoreCase) >= 0));
		Func<CardParameter, FilterParameter, bool> callAllFilters = (CardParameter card, FilterParameter param) => filters.GetInvocationList().All((Delegate func) => ((Func<CardParameter, FilterParameter, bool>)func)(card, param));
		if (alreadySorted)
		{
			return (from id in idList
				let card = cardMaster.GetCardParameterFromId(id)
				where callAllFilters(card, filterParam)
				select id).ToList();
		}
		return (from id in idList
			let card = CardMaster.GetInstance(formatBehaviour.CardMasterId).GetCardParameterFromId(id)
			where callAllFilters(card, filterParam)
			orderby new ComparableCard(card.CardId, formatBehaviour.CardMasterId)
			select id).ToList();
	}

	public List<int> SortIDList(IList<int> idList, CardMaster.CardMasterId cardMasterId)
	{
		return (from id in idList
			let card = CardMaster.GetInstance(cardMasterId).GetCardParameterFromId(id)
			orderby new ComparableCard(card.CardId, cardMasterId)
			select id).ToList();
	}

	public static CardObjData cardDataCopy(CardObjData data, CardObjData data2)
	{
		data.Cost = data2.Cost;
		data.CostNum = data2.CostNum;
		data.lifes = data2.lifes;
		data.ids = data2.ids;
		data.Atks = data2.Atks;
		data.Names = data2.Names;
		data.Skills = data2.Skills;
		data.Evo_Costs = data2.Evo_Costs;
		data.Evo_lifes = data2.Evo_lifes;
		data.Evo_Atks = data2.Evo_Atks;
		data.Evo_Names = data2.Evo_Names;
		data.Evo_Skills = data2.Evo_Skills;
		data.EvolOk = data2.EvolOk;
		data.cardType = data2.cardType;
		data.isPremiere = data2.isPremiere;
		return data;
	}

	public static Material Get2dCardMaterial(CardParameter param)
	{
		ResourcesManager.AssetLoadPathType type;
		switch (param.CharType)
		{
		case CardBasePrm.CharaType.NORMAL:
			type = ResourcesManager.AssetLoadPathType.UnitCardMaterial;
			break;
		case CardBasePrm.CharaType.FIELD:
		case CardBasePrm.CharaType.CHANT_FIELD:
		case CardBasePrm.CharaType.SPELL:
			type = (CardMaster.IsMutationCardCheck(param.BaseCardId) ? ResourcesManager.AssetLoadPathType.UnitCardMaterial : ResourcesManager.AssetLoadPathType.SpellCardMaterial);
			break;
		default:
			type = ResourcesManager.AssetLoadPathType.UnitCardMaterial;
			break;
		}
		return Toolbox.ResourcesManager.FindCardMaterial(param.ResourceCardId, type);
	}

	public void SetNumberLabelStyle(UILabel inLabel, bool inIsPremiere)
	{
		UIFont bitmapFont;
		Material material;
		if (inIsPremiere)
		{
			bitmapFont = _PremiereCardFont;
			material = _PremiereCardFont.material;
		}
		else
		{
			bitmapFont = _NormalCardFont;
			material = _NormalCardFont.material;
		}
		inLabel.bitmapFont = bitmapFont;
		inLabel.applyGradient = false;
		if (inLabel.material.name != material.name)
		{
			inLabel.material = material;
		}
	}

	public void SetNameLabelStyle(UILabel inLabel, bool inIsPremiere)
	{
		if (inIsPremiere)
		{
			inLabel.applyGradient = true;
			inLabel.gradientTop = PREMIERE_GRADIENT_TOP_COLOR;
			inLabel.gradientBottom = PREMIERE_GRADIENT_BOTTOM_COLOR;
			inLabel.effectStyle = UILabel.Effect.Outline8;
			inLabel.effectDistance = new Vector2(2f, 2f);
			inLabel.effectColor = EFFECT_COLOR;
		}
		else
		{
			inLabel.applyGradient = false;
			inLabel.effectStyle = UILabel.Effect.None;
		}
	}

	public void SetSleeveTexture(UITexture inTargetTexture, long inSleeveId)
	{
		SetSleeveTextureSub(inTargetTexture, inSleeveId, enablePremiumSleeve: true);
	}

	public void SetSleeveTextureWithoutPremium(UITexture inTargetTexture, long inSleeveId)
	{
		SetSleeveTextureSub(inTargetTexture, inSleeveId, enablePremiumSleeve: false);
	}

	private void SetSleeveTextureSub(UITexture inTargetTexture, long inSleeveId, bool enablePremiumSleeve)
	{
		inSleeveId = Toolbox.ResourcesManager.GetExistingSleeveId(inSleeveId);
		Sleeve sleeve = Data.Master.SleeveMgr.Get(inSleeveId);
		Texture texture = Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath(inSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTexture, isfetch: true)) as Texture;
		if (sleeve.IsPremiumSleeve && enablePremiumSleeve)
		{
			inTargetTexture.mainTexture = null;
			inTargetTexture.material = Toolbox.ResourcesManager.LoadObject<Material>(Toolbox.ResourcesManager.GetAssetTypePath(inSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveMaterial, isfetch: true));
			inTargetTexture.material.SetTexture("_MainTex", texture);
			Texture value = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(inSleeveId.ToString(), ResourcesManager.AssetLoadPathType.SleeveTextureMask, isfetch: true));
			inTargetTexture.material.SetTexture("_MaskTex", value);
		}
		else
		{
			inTargetTexture.mainTexture = texture;
			inTargetTexture.material = null;
		}
	}

	public void AddPremireSleevePath(ref List<string> loadPath, Sleeve sleeveData)
	{
		loadPath.Add(Toolbox.ResourcesManager.GetAssetTypePath(sleeveData.sleeve_id.ToString(), ResourcesManager.AssetLoadPathType.SleeveTextureMask));
		loadPath.Add(Toolbox.ResourcesManager.GetAssetTypePath(sleeveData.sleeve_id.ToString(), ResourcesManager.AssetLoadPathType.SleeveMaterial));
	}

	public void AddKeyWordCache(List<int> cardPool, CardMaster.CardMasterId cardMasterId)
	{
		if (_keyWordCache == null)
		{
			_keyWordCache = new CardKeyWordCache();
		}
		if (_cardNameKeyWordCache == null)
		{
			_cardNameKeyWordCache = new CardKeyWordCache(CardKeyWordCache.Option.OnlyCardNames);
		}
		if (_cardNameKeyWordHiraganaCache == null)
		{
			_cardNameKeyWordHiraganaCache = new CardKeyWordCache(CardKeyWordCache.Option.OnlyCardNamesHiranaga);
		}
		if (_keyWordCommonCache == null)
		{
			_keyWordCommonCache = new CardKeyWordCommonCache();
		}
		CardMaster instance = CardMaster.GetInstance(cardMasterId);
		foreach (int item in cardPool)
		{
			_keyWordCommonCache.CacheKeyWord(instance.GetCardParameterFromId(item));
		}
	}

	public IList<string> GetKeyword(CardParameter param)
	{
		if (_keyWordCommonCache != null)
		{
			return _keyWordCommonCache.GetCloneList(param);
		}
		return BattleKeywordInfoListMgr.GetKeywords(param);
	}
}
