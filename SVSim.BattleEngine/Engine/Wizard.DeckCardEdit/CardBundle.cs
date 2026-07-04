using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CardBundle
{

	protected CardCreator _cardCreator;

	protected Transform _parent;

	protected UITexture _sleeveOriginal;

	private List<int> _lastIdList;

	protected List<CardObject> _cardList;

	protected float _cardScale = 0.6f;

	protected bool _isDisplaySpotCardNum;

	protected bool _isHideZeroSpotCardNum;

	protected bool _canUseNonPossessionCard;

	public List<CardObject> CardList => _cardList;

	public List<int> IdList
	{
		get
		{
			List<int> ret = new List<int>(CountSum);
			_cardList.ForEach(delegate(CardObject entry)
			{
				for (int i = 0; i < entry.TotalCardNum; i++)
				{
					ret.Add(entry.CardId);
				}
			});
			return ret;
		}
	}

	public int CountSum => _cardList.Sum((CardObject card) => card.TotalCardNum);

	public virtual int CountKind => _cardList.Count;

	protected IFormatBehavior FormatBehavior { get; private set; }

	public virtual event Action<CardObject> OnCreateCard;

	public virtual event Action<CardObject> OnCreateSleeve;

	protected void OnCreateCardCall(CardObject card)
	{
		this.OnCreateCard.Call(card);
	}

	protected void OnCreateSleeveCall(CardObject sleeve)
	{
		this.OnCreateSleeve.Call(sleeve);
	}

	public void CountEachType(out int charNum, out int spellNum, out int fieldNum)
	{
		CardMaster master = CardMaster.GetInstance(FormatBehavior.CardMasterId);
		int tempCharNum = 0;
		int tempSpellNum = 0;
		int tempFieldNum = 0;
		_cardList.ForEach(delegate(CardObject entry)
		{
			switch (master.GetCardParameterFromId(entry.CardId).CharType)
			{
			case CardBasePrm.CharaType.NORMAL:
				tempCharNum += entry.TotalCardNum;
				break;
			case CardBasePrm.CharaType.SPELL:
				tempSpellNum += entry.TotalCardNum;
				break;
			case CardBasePrm.CharaType.FIELD:
			case CardBasePrm.CharaType.CHANT_FIELD:
				tempFieldNum += entry.TotalCardNum;
				break;
			}
		});
		charNum = tempCharNum;
		spellNum = tempSpellNum;
		fieldNum = tempFieldNum;
	}

	public CardBundle(CardCreator cardCreator, Transform parent, UITexture sleeveOriginal, float scale, IFormatBehavior formatBehavior, bool isDisplaySpotCardNum = false, bool isHideZeroSpotCardNum = false, bool canUseNonPossessionCard = false)
	{
		FormatBehavior = formatBehavior;
		_cardScale = scale;
		_parent = parent;
		_sleeveOriginal = sleeveOriginal;
		_cardList = new List<CardObject>();
		_cardCreator = cardCreator;
		_isDisplaySpotCardNum = isDisplaySpotCardNum;
		_isHideZeroSpotCardNum = isHideZeroSpotCardNum;
		_canUseNonPossessionCard = canUseNonPossessionCard;
	}

	public CardObject FindWithCardId(int id)
	{
		return _cardList.Find((CardObject find) => find.CardId == id);
	}

	public virtual CardObject FindWithIndex(int idx)
	{
		if (idx < 0 || idx >= _cardList.Count)
		{
			return null;
		}
		return _cardList[idx];
	}

	public CardObject FindWithObject(GameObject obj)
	{
		return _cardList.Find((CardObject find) => find.CardObj == obj);
	}

	public virtual int IndexOf(CardObject card)
	{
		return _cardList.IndexOf(card);
	}

	public virtual bool CreateCards(List<int> idList, bool isDestroyImmediate, bool isRotate, Action onCreateSleeves = null, Action onFinish = null, Action onFirstAnimationFinish = null, float cardRotateDelayTimeMax = float.MaxValue, bool isSkipSameDeckCheck = false)
	{
		if (idList == null || (!isSkipSameDeckCheck && IsSameIdList(idList, _lastIdList)))
		{
			onCreateSleeves.Call();
			onFinish.Call();
			onFirstAnimationFinish.Call();
			return false;
		}
		bool flag = false;
		List<int> orderCardIdList = idList.Distinct().ToList();
		List<CardObject> destroyList = new List<CardObject>();
		if (_canUseNonPossessionCard)
		{
			flag |= CollectOrderCardId(idList, ref orderCardIdList, ref destroyList);
			if (!flag)
			{
				onCreateSleeves.Call();
				onFinish.Call();
				onFirstAnimationFinish.Call();
				return false;
			}
			_lastIdList = idList;
			List<int> loadCardIdList = new List<int>(orderCardIdList.Count);
			_cardList = CreateOrderCardList(idList, orderCardIdList, ref loadCardIdList);
			orderCardIdList = loadCardIdList;
			if (!isDestroyImmediate)
			{
				_cardList.AddRange(destroyList);
			}
		}
		else
		{
			CardObject[] array = new CardObject[orderCardIdList.Count()];
			int count = _cardList.Count;
			flag = count != orderCardIdList.Count;
			int i;
			for (i = 0; i < count; i++)
			{
				int num = orderCardIdList.FindIndex((int card) => card == _cardList[i].CardId);
				if (num >= 0)
				{
					array[num] = _cardList[i];
					flag = flag || num != i;
				}
				else
				{
					destroyList.Add(_cardList[i]);
					flag = true;
				}
			}
			if (!flag)
			{
				onCreateSleeves.Call();
				onFinish.Call();
				onFirstAnimationFinish.Call();
				return false;
			}
			_lastIdList = idList;
			_cardList = new List<CardObject>(array);
			count = _cardList.Count;
			flag = false;
			int[] array2 = orderCardIdList.ToArray();
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				if (num2 < count && _cardList[num2] != null)
				{
					orderCardIdList.Remove(array2[num2]);
					continue;
				}
				CountUseCardNum(idList, array2[num2], out var usePossessionCardNum, out var useSpotCardNum, out var _);
				_cardList[num2] = CreateSleeve(array2[num2], usePossessionCardNum, useSpotCardNum);
				flag = true;
			}
			if (!isDestroyImmediate)
			{
				_cardList.AddRange(destroyList);
			}
			_cardList = _cardList.Distinct().ToList();
		}
		List<TweenAlpha> tweenList = new List<TweenAlpha>();
		for (int num3 = 0; num3 < destroyList.Count; num3++)
		{
			CardObject cardObject = destroyList[num3];
			UITexture cardTexture = cardObject.CardObj.GetComponent<CardListTemplate>()._cardTexture;
			if ((bool)cardTexture && (bool)cardTexture.material && (bool)cardTexture.material.mainTexture)
			{
				Texture mainTexture = cardTexture.material.mainTexture;
				if (!cardObject.IsNonPossessionCard)
				{
					cardTexture.material = null;
					cardTexture.mainTexture = mainTexture;
				}
				cardTexture.depth--;
				tweenList.Add(TweenAlpha.Begin(cardObject.CardObj, 0.3f, 0f));
			}
		}
		onCreateSleeves.Call();
		Load(orderCardIdList, isPreferentially: true, delegate(List<UIBase_CardManager.CardObjData> cardObjDataList)
		{
			tweenList.ForEach(delegate(TweenAlpha tween)
			{
				UnityEngine.Object.Destroy(tween);
			});
			if (_lastIdList == idList)
			{
				_cardList.RemoveAll((CardObject card) => destroyList.Contains(card));
				destroyList.ForEach(DestroyEach);
				Func<int, CardObject> func = null;
				func = ((!_canUseNonPossessionCard) ? ((Func<int, CardObject>)((int cardId) => _cardList.Find((CardObject c) => c.CardId == cardId))) : ((Func<int, CardObject>)((int cardId) => _cardList.Find((CardObject c) => c.CardId == cardId && !c.IsAttachedCardObjData))));
				float num4 = 0f;
				int count2 = cardObjDataList.Count;
				for (int num5 = 0; num5 < count2; num5++)
				{
					cardObjDataList[num5].ids = orderCardIdList[num5];
					CardObject cardObject2 = func(orderCardIdList[num5]);
					if (cardObject2 != null)
					{
						if (cardObject2.IsVisibleSleeve)
						{
							int mainCardNum = cardObject2.MainCardNum;
							int subCardNum = cardObject2.SubCardNum;
							bool activeInHierarchy = cardObject2.CardObj.activeInHierarchy;
							cardObject2.CompleteSleeveTweenAlpha();
							cardObject2.AttachCardObjData(cardObjDataList[num5]);
							cardObject2.MainCardNum = mainCardNum;
							cardObject2.SubCardNum = subCardNum;
							if (isRotate && activeInHierarchy)
							{
								MonoBehaviour component = cardObjDataList[num5].CardObj.GetComponent<MonoBehaviour>();
								cardObject2.ActiveCullObjs(isActive: false);
								component.StartCoroutine(RotateAndTakeoffSleeve(cardObject2, num4));
								num4 = Mathf.Min(num4 + 0.05f, cardRotateDelayTimeMax);
							}
							else
							{
								cardObject2.TakeOffSleeve();
								cardObject2.CardObj.SetActive(activeInHierarchy);
								this.OnCreateCard.Call(cardObject2);
							}
						}
						if (cardObject2.IsNonPossessionCard)
						{
							cardObject2.AttachGrayShader();
						}
					}
				}
				if (num4 > 0f)
				{

				}
				if (isRotate)
				{
					UIManager.GetInstance().StartCoroutine(DelayEventCall(num4, onFirstAnimationFinish));
				}
				else
				{
					onFirstAnimationFinish.Call();
				}
				onFinish.Call();
			}
		});
		return flag;
	}

	private bool IsSameIdList(List<int> a, List<int> b)
	{
		if (a == null)
		{
			return b != null;
		}
		if (b != null)
		{
			return a.SequenceEqual(b);
		}
		return false;
	}

	private IEnumerator DelayEventCall(float delay, Action finishEvent)
	{
		yield return new WaitForSeconds(delay);
		finishEvent.Call();
	}

	private bool CollectOrderCardId(List<int> idList, ref List<int> orderCardIdList, ref List<CardObject> destroyList)
	{
		bool flag = false;
		for (int num = orderCardIdList.Count - 1; num >= 0; num--)
		{
			int cardId = orderCardIdList[num];
			List<CardObject> list = _cardList.FindAll((CardObject c) => c.CardId == cardId);
			CardObject cardObject = list.Find((CardObject c) => !c.IsNonPossessionCard);
			CardObject cardObject2 = list.Find((CardObject c) => c.IsNonPossessionCard);
			int possessionCardNum = FormatBehavior.GetPossessionCardNum(cardId, _isDisplaySpotCardNum);
			if (possessionCardNum > 0)
			{
				if (possessionCardNum < idList.Count((int id) => id == cardId))
				{
					flag = flag || cardObject == null || cardObject2 == null;
					orderCardIdList.Insert(num, cardId);
				}
				else
				{
					flag = flag || cardObject == null || cardObject2 != null;
					if (cardObject2 != null)
					{
						destroyList.Add(cardObject2);
					}
				}
			}
			else
			{
				flag = flag || cardObject != null || cardObject2 == null;
				if (cardObject != null)
				{
					destroyList.Add(cardObject);
				}
			}
		}
		flag |= orderCardIdList.Count != _cardList.Count;
		foreach (CardObject card in _cardList)
		{
			if (!destroyList.Contains(card) && !orderCardIdList.Contains(card.CardId))
			{
				destroyList.Add(card);
			}
		}
		return flag | (destroyList.Count > 0);
	}

	private List<CardObject> CreateOrderCardList(List<int> idList, List<int> orderCardIdList, ref List<int> loadCardIdList)
	{
		List<CardObject> list = new List<CardObject>(orderCardIdList.Count);
		int num = -1;
		for (int i = 0; i < orderCardIdList.Count; i++)
		{
			int cardId = orderCardIdList[i];
			bool isNonPossessionCard = FormatBehavior.GetPossessionCardNum(cardId, isIncludingSpotCard: true) == 0 || cardId == num;
			CountMainAndSubNum(idList, cardId, isNonPossessionCard, out var mainNum, out var subNum);
			CardObject cardObject = _cardList.Find((CardObject c) => c.CardId == cardId && c.IsNonPossessionCard == isNonPossessionCard);
			if (cardObject != null)
			{
				list.Add(cardObject);
				cardObject.MainCardNum = mainNum;
				cardObject.SubCardNum = subNum;
			}
			else
			{
				list.Add(CreateSleeve(cardId, mainNum, subNum, isNonPossessionCard));
				loadCardIdList.Add(cardId);
			}
			num = cardId;
		}
		return list;
	}

	public virtual CardObject Insert(CardObject card, bool dontCreate)
	{
		if (card == null)
		{
			return null;
		}
		_lastIdList = null;
		CardObject cardObject = null;
		bool isNonPossessionCard = false;
		if (_canUseNonPossessionCard)
		{
			int num = IdList.Count((int id) => id == card.CardId);
			isNonPossessionCard = num >= FormatBehavior.GetPossessionCardNum(card.CardId, isIncludingSpotCard: true);
			cardObject = _cardList.Find((CardObject c) => c.CardId == card.CardId && c.IsNonPossessionCard == isNonPossessionCard);
		}
		else
		{
			cardObject = _cardList.Find((CardObject find) => find.CardId == card.CardId);
		}
		int possessionCardNum = FormatBehavior.GetPossessionCardNum(card.CardId, isIncludingSpotCard: false);
		if (cardObject != null)
		{
			if (cardObject.MainCardNum < possessionCardNum || isNonPossessionCard)
			{
				cardObject.MainCardNum++;
			}
			else
			{
				cardObject.SubCardNum++;
			}
			return cardObject;
		}
		if (!dontCreate)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstance(FormatBehavior.CardMasterId).GetCardParameterFromId(card.CardId);
			Toolbox.ResourcesManager.StartCoroutine(Toolbox.ResourcesManager.LoadAssetAsync(Toolbox.ResourcesManager.GetAssetTypePath(cardParameterFromId.ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial), null));
			if (cardParameterFromId.IsFoil)
			{
				int resourceCardId = CardMaster.GetInstance(FormatBehavior.CardMasterId).GetCardParameterFromId(cardParameterFromId.NormalCardId).ResourceCardId;
				Toolbox.ResourcesManager.StartCoroutine(Toolbox.ResourcesManager.LoadAssetAsync(Toolbox.ResourcesManager.GetAssetTypePath(resourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial), null));
			}
			Vector3 position = card.CardObj.transform.position;
			CardObject cardObject2 = card.Clone();
			card.ActiveCardInfo(isActive: false);
			cardObject2.CardObj = UnityEngine.Object.Instantiate(card.CardObj);
			card.ActiveCardInfo(isActive: true);
			cardObject2.CardObj.transform.position = position;
			cardObject2.SetScale(_cardScale);
			CardListTemplate component = cardObject2.CardObj.GetComponent<CardListTemplate>();
			if (component != null)
			{
				component.RotationOnlyIconVisible = cardParameterFromId.IsResurgentCard;
			}
			cardObject2.IsDisplaySpotCardNum = _isDisplaySpotCardNum;
			cardObject2.IsHideZeroSpotCardNum = _isHideZeroSpotCardNum;
			cardObject2.IsNonPossessionCard = isNonPossessionCard;
			if (possessionCardNum > 0 || isNonPossessionCard)
			{
				cardObject2.MainCardNum = 1;
				cardObject2.SubCardNum = 0;
			}
			else
			{
				cardObject2.MainCardNum = 0;
				cardObject2.SubCardNum = 1;
			}
			if (isNonPossessionCard)
			{
				cardObject2.AttachGrayShader();
			}
			_cardList.Add(cardObject2);
			_cardList = (from c in _cardList
				orderby new UIBase_CardManager.ComparableCard(c.CardId, FormatBehavior.CardMasterId), c.IsNonPossessionCard
				select c).ToList();
			cardObject2.CardObj.GetComponent<CharIdx>().SetCardId(cardObject2.CardId);
			this.OnCreateCard.Call(cardObject2);
			return cardObject2;
		}
		return null;
	}

	public virtual CardObject Remove(CardObject card, bool isDestroyObject)
	{
		if (card == null)
		{
			return null;
		}
		_lastIdList = null;
		CardObject cardObject = _cardList.Find((CardObject find) => find == card);
		if (cardObject == null)
		{
			return null;
		}
		if (cardObject.SubCardNum > 0 && cardObject.MainCardNum <= FormatBehavior.GetPossessionCardNum(card.CardId, isIncludingSpotCard: false) && !cardObject.IsNonPossessionCard)
		{
			cardObject.SubCardNum--;
		}
		else
		{
			cardObject.MainCardNum--;
		}
		if (cardObject.TotalCardNum == 0)
		{
			if (isDestroyObject)
			{
				_cardList.Remove(card);
				DestroyEach(card);
				return null;
			}
			return cardObject;
		}
		return cardObject;
	}

	public virtual void DestroyAll()
	{
		_cardList.ForEach(delegate(CardObject card)
		{
			card.Destroy();
		});
		_cardList.Clear();
		_lastIdList = null;
	}

	protected virtual void DestroyEach(CardObject card)
	{
		card.Destroy();
	}

	protected virtual CardObject CreateSleeve(int id, int mainNum, int subNum, bool isNonPossessionCard = false)
	{
		CardObject cardObject = new CardObject(_parent, _cardScale, FormatBehavior, _isDisplaySpotCardNum, _isHideZeroSpotCardNum, isNonPossessionCard);
		cardObject.ShowSleeve(_sleeveOriginal);
		cardObject.CardId = id;
		cardObject.MainCardNum = mainNum;
		cardObject.SubCardNum = subNum;
		this.OnCreateSleeve.Call(cardObject);
		return cardObject;
	}

	public virtual void Load(List<int> order, bool isPreferentially, Action<List<UIBase_CardManager.CardObjData>> onFinish)
	{
		Func<List<int>> order2 = () => order;
		_cardCreator.Request(order2, isPreferentially, onFinish, FormatBehavior.CardMasterId);
	}

	private IEnumerator RotateAndTakeoffSleeve(CardObject card, float delay)
	{
		card.NotifyRotateAnimation();
		yield return new WaitForSeconds(delay);
		card.RotateAnim(delegate
		{
			card.TakeOffSleeve();
			this.OnCreateCard.Call(card);
		});
	}

	public void CountUseCardNum(List<int> cardIdList, int cardId, out int usePossessionCardNum, out int useSpotCardNum, out int useNonPossessionCardNum)
	{
		int possessionCardNum = FormatBehavior.GetPossessionCardNum(cardId, isIncludingSpotCard: false);
		int val = FormatBehavior.GetPossessionCardNum(cardId, isIncludingSpotCard: true) - possessionCardNum;
		int num = cardIdList.Count((int id) => id == cardId);
		usePossessionCardNum = Math.Min(possessionCardNum, num);
		useSpotCardNum = Math.Max(0, Math.Min(num - usePossessionCardNum, val));
		useNonPossessionCardNum = num - usePossessionCardNum - useSpotCardNum;
	}

	public void CountMainAndSubNum(List<int> cardIdList, int cardId, bool isNonPossessionCard, out int mainNum, out int subNum)
	{
		CountUseCardNum(cardIdList, cardId, out var usePossessionCardNum, out var useSpotCardNum, out var useNonPossessionCardNum);
		if (isNonPossessionCard)
		{
			mainNum = useNonPossessionCardNum;
			subNum = 0;
		}
		else
		{
			mainNum = usePossessionCardNum;
			subNum = useSpotCardNum;
		}
	}

	public void CountMainAndSubNum(int cardId, bool isNonPossessionCard, out int mainNum, out int subNum)
	{
		CountMainAndSubNum(IdList, cardId, isNonPossessionCard, out mainNum, out subNum);
	}

	public int CountClassCard(CardBasePrm.ClanType classId)
	{
		CardMaster cardMaster = CardMaster.GetInstance(FormatBehavior.CardMasterId);
		return _cardList.Where((CardObject card) => cardMaster.GetCardParameterFromId(card.CardId).Clan == classId).Sum((CardObject card) => card.TotalCardNum);
	}
}
