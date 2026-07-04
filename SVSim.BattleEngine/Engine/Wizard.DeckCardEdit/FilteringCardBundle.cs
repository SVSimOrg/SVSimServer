using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard.DeckCardEdit;

public class FilteringCardBundle : CardBundle
{
	private UIBase_CardManager.FilterParameter _filter;

	private List<CardObject> _filteringList;

	public override int CountKind => _filteringList.Count;

	public int CountKindNoFilter => base.CountKind;

	public FilteringCardBundle(CardCreator cardCreator, Transform parent, UITexture sleeveOriginal, float scale, IFormatBehavior formatBehavior, bool isDisplaySpotCardNum, bool isHideZeroSpotCardNum, bool canUseNonPossessionCard)
		: base(cardCreator, parent, sleeveOriginal, scale, formatBehavior, isDisplaySpotCardNum, isHideZeroSpotCardNum, canUseNonPossessionCard)
	{
		_filter = new UIBase_CardManager.FilterParameter();
		_filteringList = new List<CardObject>();
	}

	public void ApplyFilter(UIBase_CardManager.FilterParameter filter)
	{
		_filter = filter;
		List<int> idList = UIManager.GetInstance().getUIBase_CardManager().SelectCardIDInConditionMask(base.IdList.Distinct().ToList(), filter, base.FormatBehavior, null)
			.ToList();
		_cardList.ForEach(delegate(CardObject card)
		{
			card.CardObj.SetActive(value: false);
		});
		_filteringList = new List<CardObject>();
		_cardList.ForEach(delegate(CardObject card)
		{
			if (idList.Contains(card.CardId))
			{
				_filteringList.Add(card);
				card.CardObj.SetActive(value: true);
			}
			else
			{
				card.CardObj.transform.localPosition = Vector3.zero;
				card.CardObj.SetActive(value: false);
			}
		});
	}

	public override CardObject FindWithIndex(int idx)
	{
		if (idx < 0 || idx >= _filteringList.Count)
		{
			return null;
		}
		return _filteringList[idx];
	}

	public CardObject FindWithIndexNoFilter(int idx)
	{
		return base.FindWithIndex(idx);
	}

	public override int IndexOf(CardObject card)
	{
		return _filteringList.IndexOf(card);
	}

	public override bool CreateCards(List<int> idList, bool isDestroyImmediate, bool isRotate, Action onCreateSleeves = null, Action onFinish = null, Action onFirstAnimationFinish = null, float cardRotateDelayTimeMax = float.MaxValue, bool isSkipSameDeckCheck = false)
	{
		return base.CreateCards(idList, isDestroyImmediate, isRotate, delegate
		{
			ApplyFilter(_filter);
			onCreateSleeves.Call();
		}, onFinish, onFirstAnimationFinish, cardRotateDelayTimeMax, isSkipSameDeckCheck);
	}

	public override CardObject Insert(CardObject card, bool dontCreate)
	{
		CardObject result = base.Insert(card, dontCreate);
		ApplyFilter(_filter);
		return result;
	}

	public override CardObject Remove(CardObject card, bool isDestroyObject)
	{
		CardObject result = base.Remove(card, isDestroyObject);
		ApplyFilter(_filter);
		return result;
	}

	public override void DestroyAll()
	{
		base.DestroyAll();
		_filteringList.Clear();
	}
}
