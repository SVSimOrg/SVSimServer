using System;
using System.Collections;
using UnityEngine;

public class MecanimSceneBase : UIBase
{

	protected Animator m_animator;

	protected MecanimStateBase m_state;

	protected MecanimStateBase m_nextState;

	private Coroutine m_waitCoroutine;

	private Action m_callback;

	public MecanimStateBase CurrentState => m_state;

	public void ChangeState(MecanimStateBase state, bool skipCloseAnim = false, bool skipOpenAnim = false)
	{
		Action nextFunc = delegate
		{
			if ((bool)(m_state = m_nextState))
			{
				m_state.onOpen();
				PlayAndCallback(m_state.OpenStateName, m_state.onUpdateOpenAnim, delegate
				{
					if ((bool)m_state)
					{
						m_state.onFinishOpenAnim();
						if (m_state.NextState != null)
						{
							ChangeState(m_state.NextState);
						}
					}
				}, skipOpenAnim);
			}
		};
		if ((bool)state && !state.onOpenRequest(m_state, m_callback != null))
		{
			return;
		}
		if ((bool)m_state)
		{
			if (!m_state.onCloseRequest(state, m_callback != null))
			{
				return;
			}
			m_nextState = state;
			m_state.onClose();
			PlayAndCallback(m_state.CloseStateName, m_state.onUpdateCloseAnim, delegate
			{
				if ((bool)m_state)
				{
					m_state.onFinishCloseAnim();
					nextFunc();
				}
			}, skipCloseAnim);
		}
		else
		{
			m_nextState = state;
			nextFunc();
		}
	}

	private bool Play(string state, bool isSkip = false)
	{
		if (!m_animator)
		{
			return false;
		}
		if (m_waitCoroutine != null)
		{
			StopCoroutine(m_waitCoroutine);
			m_waitCoroutine = null;
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			m_animator.Play(currentAnimatorStateInfo.fullPathHash, 0, 1f);
			if (m_callback != null)
			{
				Action callback = m_callback;
				m_callback = null;
				callback();
			}
		}
		if (state != null && state.Length > 0)
		{
			m_animator.Play(state, 0, isSkip ? 1f : 0f);
			return true;
		}
		return false;
	}

	private bool PlayAndCallback(string state, Action update, Action callback, bool isSkip = false)
	{
		bool num = Play(state, isSkip);
		if (num && !isSkip)
		{
			m_callback = callback;
			m_waitCoroutine = StartCoroutine(PlayAndCallback_Coroutine(state, update));
			return num;
		}
		callback?.Invoke();
		return num;
	}

	private IEnumerator PlayAndCallback_Coroutine(string state, Action update)
	{
		if ((bool)m_animator)
		{
			AnimatorStateInfo info;
			while (true)
			{
				AnimatorStateInfo currentAnimatorStateInfo;
				info = (currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0));
				currentAnimatorStateInfo = currentAnimatorStateInfo;
				if (currentAnimatorStateInfo.IsName(state))
				{
					break;
				}
				yield return null;
			}
			float time = 0f - Time.deltaTime;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime);
				if (!(num < info.length))
				{
					break;
				}
				update?.Invoke();
				yield return null;
			}
		}
		m_waitCoroutine = null;
		if (m_callback != null)
		{
			Action callback = m_callback;
			m_callback = null;
			callback();
		}
	}

	public override void onFirstStart()
	{
		base.onFirstStart();
		if ((bool)(m_animator = GetComponent<Animator>()))
		{
			Play("FirstStart");
		}
	}

	protected override void onOpen()
	{
		base.onOpen();
		if ((bool)m_animator)
		{
			PlayAndCallback("Open", onUpdateOpenAnim, delegate
			{
				onFinishOpenAnim();
			});
		}
		else
		{
			onFinishOpenAnim();
		}
	}

	protected virtual void onUpdateOpenAnim()
	{
	}

	protected virtual void onFinishOpenAnim()
	{
	}

	protected override void onClose()
	{
		m_state = null;
		if ((bool)m_animator)
		{
			PlayAndCallback("Close", onUpdateCloseAnim, delegate
			{
				onFinishCloseAnim();
			});
		}
		else
		{
			onFinishCloseAnim();
		}
	}

	protected virtual void onUpdateCloseAnim()
	{
	}

	protected virtual void onFinishCloseAnim()
	{
		base.onClose();
	}

	public override void onMove()
	{
		base.onMove();
		if ((bool)m_state && !m_state.DontMove)
		{
			m_state.onMove();
		}
	}
}
