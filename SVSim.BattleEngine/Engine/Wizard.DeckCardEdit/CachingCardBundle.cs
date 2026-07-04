using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class CachingCardBundle : CardBundle
{

	private List<UIBase_CardManager.CardObjData> _cachedList;

	public CachingCardBundle(CardCreator cardCreator, Transform parent, UITexture sleeveOriginal, float scale, IFormatBehavior formatBehavior, bool isDisplaySpotCardNum)
		: base(cardCreator, parent, sleeveOriginal, scale, formatBehavior, isDisplaySpotCardNum)
	{
		_cachedList = new List<UIBase_CardManager.CardObjData>(32);
	}

	private UIBase_CardManager.CardObjData GetCachedCardFromId(int id)
	{
		return _cachedList.Find((UIBase_CardManager.CardObjData card) => card.ids == id);
	}

	private void AddCache(UIBase_CardManager.CardObjData item)
	{
		_cachedList.Remove(item);
		_cachedList.Insert(0, item);
	}

	private void RemoveCache(UIBase_CardManager.CardObjData item)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(item.ids);
		Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(cardParameterFromId.ResourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial));
		if (cardParameterFromId.IsFoil)
		{
			int resourceCardId = CardMaster.GetInstance(base.FormatBehavior.CardMasterId).GetCardParameterFromId(cardParameterFromId.NormalCardId).ResourceCardId;
			Toolbox.ResourcesManager.RemoveAsset(Toolbox.ResourcesManager.GetAssetTypePath(resourceCardId.ToString(), ResourcesManager.AssetLoadPathType.UnitCardMaterial));
		}
		UnityEngine.Object.Destroy(item.CardObj);
		_cachedList.Remove(item);
	}

	public override bool CreateCards(List<int> idList, bool isDestroyImmediate, bool isRotate, Action onCreateSleeves = null, Action onFinish = null, Action onFirstAnimationFinish = null, float cardRotateDelayTimeMax = float.MaxValue, bool isSkipSameDeckCheck = false)
	{
		int count = idList.Count;
		for (int i = 0; i < count; i++)
		{
			UIBase_CardManager.CardObjData cachedCardFromId = GetCachedCardFromId(idList[i]);
			if (cachedCardFromId != null)
			{
				AddCache(cachedCardFromId);
			}
		}
		return base.CreateCards(idList, isDestroyImmediate, isRotate, onCreateSleeves, onFinish, onFirstAnimationFinish, cardRotateDelayTimeMax, isSkipSameDeckCheck);
	}

	protected override CardObject CreateSleeve(int id, int mainNum, int subNum, bool isNonPossessionCard = false)
	{
		UIBase_CardManager.CardObjData cachedCardFromId = GetCachedCardFromId(id);
		if (cachedCardFromId != null)
		{
			CardObject cardObject = new CardObject(cachedCardFromId, _parent, _cardScale, base.FormatBehavior, _isDisplaySpotCardNum, _isHideZeroSpotCardNum);
			cardObject.CardObj.SetActive(value: true);
			cardObject.CardId = id;
			cardObject.MainCardNum = mainNum;
			cardObject.SubCardNum = subNum;
			OnCreateSleeveCall(cardObject);
			OnCreateCardCall(cardObject);
			return cardObject;
		}
		return base.CreateSleeve(id, mainNum, subNum);
	}

	public override void Load(List<int> order, bool isPreferentially, Action<List<UIBase_CardManager.CardObjData>> onFinish)
	{
		int orderLen = order.Count;
		Func<List<int>> order2 = () => order.Where((int id) => GetCachedCardFromId(id) == null).ToList();
		_cardCreator.Request(order2, isPreferentially, delegate(List<UIBase_CardManager.CardObjData> returnList)
		{
			int i;
			for (i = 0; i < orderLen; i++)
			{
				UIBase_CardManager.CardObjData cardObjData = returnList.Find((UIBase_CardManager.CardObjData entry) => entry.ids == order[i]);
				UIBase_CardManager.CardObjData cachedCardFromId = GetCachedCardFromId(order[i]);
				if (cachedCardFromId != null)
				{
					if (cardObjData != null)
					{
						UnityEngine.Object.Destroy(cardObjData.CardObj);
						returnList.Remove(cardObjData);
					}
					cachedCardFromId.CardObj.SetActive(value: true);
					cachedCardFromId.CardObj.GetComponent<UIWidget>().alpha = 1f;
					returnList.Insert(i, cachedCardFromId);
				}
				AddCache(returnList[i]);
			}
			DestroyOverStock();
			onFinish.Call(returnList);
		}, base.FormatBehavior.CardMasterId);
	}

	private void DestroyOverStock()
	{
		int num = _cachedList.Count();
		if (num <= 32)
		{
			return;
		}
		int num2 = num - 32;
		for (int i = 0; i < num; i++)
		{
			if (_cachedList.Count() <= 0)
			{
				break;
			}
			if (num2 <= 0)
			{
				break;
			}
			RemoveCache(_cachedList[_cachedList.Count() - 1]);
			num2--;
		}
	}

	public override void DestroyAll()
	{
		_cardList.ForEach(delegate(CardObject card)
		{
			card.Destroy(isRemoveAsset: false);
		});
		_cardList.Clear();
		while (_cachedList.Count > 0)
		{
			RemoveCache(_cachedList[0]);
		}
	}

	protected override void DestroyEach(CardObject card)
	{
		card.DestroySleeve();
		card.DestroyUseInfo();
		card.DestroyCursorEffect();
		card.DestroyTween();
		card.ActiveCullObjs(isActive: true);
		card.CardObj.SetActive(value: false);
	}
}
