using System;
using UnityEngine;

namespace Wizard;

public abstract class ParameterOverwriterBase : MonoBehaviour
{
	private enum State
	{
		None,
		RequestOverwrite	}

	private State _state;

	protected abstract GameObject TargetObject { get; }

	private void Awake()
	{
		SaveOriginalParameters();
		_state = State.None;
	}

	protected void RequestOverwrite()
	{
		if (base.enabled)
		{
			_state = State.RequestOverwrite;
		}
	}

	protected bool ColorTryParse(string strColorCodeId, out Color color)
	{
		color = Color.red;
		if (string.IsNullOrEmpty(strColorCodeId))
		{
			return false;
		}
		if (Enum.TryParse<eColorCodeId>(strColorCodeId, out var result))
		{
			color = ColorCode.Get(result);
			return true;
		}
		return false;
	}

	protected abstract void SaveOriginalParameters();

	protected abstract void UndoParameters();

	protected abstract void OverwriteParameters();
}
