using System;

namespace Cute;

public class ParallelJob
{
	private Action _action;

	public bool isDone { get; private set; }

	public static ParallelJob Dispatch(Action action)
	{
		ParallelJob parallelJob = new ParallelJob(action);
		LeanThreadPool.Instance.AddJob(parallelJob);
		return parallelJob;
	}

	private ParallelJob(Action action)
	{
		isDone = false;
		_action = action;
	}

	internal void Run()
	{
		if (!isDone)
		{
			if (_action != null)
			{
				_action();
				_action = null;
			}
			isDone = true;
		}
	}
}
