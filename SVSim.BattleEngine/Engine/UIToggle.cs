using System;
using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Toggle")]
public class UIToggle : UIWidgetContainer
{
	public delegate bool Validate(bool choice);

	public static BetterList<UIToggle> list = new BetterList<UIToggle>();

	public static UIToggle current;

	public int group;

	public UIWidget activeSprite;

	public Animation activeAnimation;

	public Animator animator;

	public UITweener tween;

	public bool startsActive;

	public bool instantTween;

	public bool optionCanBeNone;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public Validate validator;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnActivate";

	private bool mIsActive = true;

	private bool mStarted;

	public bool value
	{
		get
		{
			if (!mStarted)
			{
				return startsActive;
			}
			return mIsActive;
		}
		set
		{
			if (!mStarted)
			{
				startsActive = value;
			}
			else if (group == 0 || value || optionCanBeNone || !mStarted)
			{
				Set(value);
			}
		}
	}

	public void Set(bool state)
	{
		if (validator != null && !validator(state))
		{
			return;
		}
		if (!mStarted)
		{
			mIsActive = state;
			startsActive = state;
			if (activeSprite != null)
			{
				activeSprite.alpha = (state ? 1f : 0f);
			}
		}
		else
		{
			if (mIsActive == state)
			{
				return;
			}
			if (group != 0 && state)
			{
				int num = 0;
				int size = list.size;
				while (num < size)
				{
					UIToggle uIToggle = list[num];
					if (uIToggle != this && uIToggle.group == group)
					{
						uIToggle.Set(state: false);
					}
					if (list.size != size)
					{
						size = list.size;
						num = 0;
					}
					else
					{
						num++;
					}
				}
			}
			mIsActive = state;
			if (activeSprite != null)
			{
				if (instantTween || !NGUITools.GetActive(this))
				{
					activeSprite.alpha = (mIsActive ? 1f : 0f);
				}
				else
				{
					TweenAlpha.Begin(activeSprite.gameObject, 0.15f, mIsActive ? 1f : 0f);
				}
			}
			if (current == null)
			{
				UIToggle uIToggle2 = current;
				current = this;
				if (EventDelegate.IsValid(onChange))
				{
					EventDelegate.Execute(onChange);
				}
				else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
				{
					eventReceiver.SendMessage(functionName, mIsActive, SendMessageOptions.DontRequireReceiver);
				}
				current = uIToggle2;
			}
			if (animator != null)
			{
				ActiveAnimation activeAnimation = ActiveAnimation.Play(animator, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if (activeAnimation != null && (instantTween || !NGUITools.GetActive(this)))
				{
					activeAnimation.Finish();
				}
			}
			else if (this.activeAnimation != null)
			{
				ActiveAnimation activeAnimation2 = ActiveAnimation.Play(this.activeAnimation, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if (activeAnimation2 != null && (instantTween || !NGUITools.GetActive(this)))
				{
					activeAnimation2.Finish();
				}
			}
			else
			{
				if (!(tween != null))
				{
					return;
				}
				bool active = NGUITools.GetActive(this);
				if (tween.tweenGroup != 0)
				{
					UITweener[] componentsInChildren = tween.GetComponentsInChildren<UITweener>();
					int i = 0;
					for (int num2 = componentsInChildren.Length; i < num2; i++)
					{
						UITweener uITweener = componentsInChildren[i];
						if (uITweener.tweenGroup == tween.tweenGroup)
						{
							uITweener.Play(state);
							if (instantTween || !active)
							{
								uITweener.tweenFactor = (state ? 1f : 0f);
							}
						}
					}
				}
				else
				{
					tween.Play(state);
					if (instantTween || !active)
					{
						tween.tweenFactor = (state ? 1f : 0f);
					}
				}
			}
		}
	}
}
