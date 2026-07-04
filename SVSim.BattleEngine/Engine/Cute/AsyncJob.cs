using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cute;

public class AsyncJob
{
	private class Unit
	{
		public object action;

		public Action cancelAction;

		public Unit(object action, Action cancelAction)
		{
			this.action = action;
			this.cancelAction = cancelAction;
		}
	}

	private int num;

	private MonoBehaviour mono;

	private List<Unit> jobList = new List<Unit>();

	private bool isCancel;

	public AsyncJob(MonoBehaviour mono, int num)
	{
		this.mono = mono;
		this.num = num;
	}

	public void Add(IEnumerator enumerator, Action calcelAction)
	{
		jobList.Add(new Unit(enumerator, calcelAction));
	}

	public void Add(Action action, Action calcelAction)
	{
		jobList.Add(new Unit(action, calcelAction));
	}

	public void Cancel()
	{
		if (!isCancel && jobList.Count != 0)
		{
			isCancel = true;
			Add(CancelTerminator, CancelTerminator);
		}
	}

	public void CancelTerminator()
	{
		isCancel = false;
	}
}
