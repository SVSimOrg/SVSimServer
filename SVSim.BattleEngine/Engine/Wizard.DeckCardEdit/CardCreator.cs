using System;
using System.Collections.Generic;
using Cute;

namespace Wizard.DeckCardEdit;

public class CardCreator
{
	private class Task
	{
		public List<int> Order;

		public Action<List<UIBase_CardManager.CardObjData>> OnFinish;

		public Task(List<int> order, Action<List<UIBase_CardManager.CardObjData>> onFinish)
		{
			Order = order;
			OnFinish = onFinish;
		}
	}

	private bool _isBusy;

	private List<Task> _taskQueue;

	private CardMaster.CardMasterId _cardMasterId;

	public event Action OnFinishedLatest;

	public CardCreator()
	{
		_taskQueue = new List<Task>(8);
	}

	public void Request(Func<List<int>> order, bool isPreferentially, Action<List<UIBase_CardManager.CardObjData>> onFinish, CardMaster.CardMasterId cardMasterId)
	{
		_cardMasterId = cardMasterId;
		if (isPreferentially)
		{
			_taskQueue.Insert(0, new Task(order(), onFinish));
		}
		else
		{
			_taskQueue.Add(new Task(order(), onFinish));
		}
	}

	public void Tick()
	{
		if (!_isBusy)
		{
			StartCreate(Dequeue());
		}
	}

	public void Clear()
	{
		_taskQueue.Clear();
	}

	private void StartCreate(Task task)
	{
		if (task != null)
		{
			_isBusy = true;
			Toolbox.ResourcesManager.CardListAssetPathList.Clear();
			UIManager.GetInstance().CardLoadSelect(null, task.Order, 0, is2D: true, delegate
			{
				task.OnFinish.Call(UIManager.GetInstance().getCardList2DObjs());
				UIManager.GetInstance().getCardList2DObjs().Clear();
				_isBusy = false;
				this.OnFinishedLatest.Call();
			}, isDefaultSleeve: false, _cardMasterId);
		}
	}

	private Task Dequeue()
	{
		Task result = null;
		if (_taskQueue.Count > 0)
		{
			result = _taskQueue[0];
			_taskQueue.RemoveAt(0);
		}
		return result;
	}
}
