using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;

public class CardMake : MonoBehaviour
{

	private IDictionary<string, string> _craftDict;

	public Action OnCardBuy;

	public Action OnClose;

	private void CraftCard()
	{
		// Pre-Phase-5b: fired CardCreateTask via NetworkManager. See DestructCard.
		OnRequestFinishCraft(default(NetworkTask.ResultCode));
	}

	private void OnRequestFinishCraft(NetworkTask.ResultCode error)
	{
		if (OnCardBuy != null)
		{
			OnCardBuy();
		}
		if (OnClose != null)
		{
			OnClose();
		}
	}

	public void StartCraftAll(IDictionary<int, int> craftDict)
	{
		if (_craftDict == null)
		{
			_craftDict = new Dictionary<string, string>();
		}
		else
		{
			_craftDict.Clear();
		}
		foreach (KeyValuePair<int, int> item in craftDict)
		{
			if (item.Value > 0)
			{
				string value = CreateRequestParam(item.Key, item.Value);
				_craftDict.Add(item.Key.ToString(), value);
			}
		}
		if (_craftDict.Count > 0)
		{
			CraftCard();
		}
	}

	private string CreateRequestParam(int cardId, int num)
	{
		int possessionCardNum = 0; // Pre-Phase-5b: headless has no user inventory
		return $"{num},{possessionCardNum}";
	}
}
