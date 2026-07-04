using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.Resource;
using Wizard.Battle.Touch;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public class PlayerEmotion : EmotionBase, IPlayerEmotion, IEmotion
{
	private class WaitFlagChangeVfx : VfxBase
	{
		private readonly Func<bool> _isEnd;

		public override bool IsEnd
		{
			get
			{
				return _isEnd();
			}
			protected set
			{
			}
		}

		public WaitFlagChangeVfx(Func<bool> isEnd)
		{
			_isEnd = isEnd;
		}
	}

	private static readonly string[] FOCUS_ICON_NAMES = new string[7] { "battle_btn_emote_greet_on", "battle_btn_emote_thank_on", "battle_btn_emote_apology_on", "battle_btn_emote_praise_on", "battle_btn_emote_surprise_on", "battle_btn_emote_confuse_on", "battle_btn_emote_provocation_on" };

	private static readonly string[] UNFOCUS_ICON_NAMES = new string[7] { "battle_btn_emote_greet_off", "battle_btn_emote_thank_off", "battle_btn_emote_apology_off", "battle_btn_emote_praise_off", "battle_btn_emote_surprise_off", "battle_btn_emote_confuse_off", "battle_btn_emote_provocation_off" };

	private int _playCount;

	private readonly IBattleResourceMgr _resourceMgr;

	private GameObject[] _iconObjects;

	private GameObject _currentFocusIconObject;

	private bool _isIconShowReserve;

	private bool _isActiveHideTween;

	private bool _debugLogLimitter;

	public IEnumerable<GameObject> IconObjects => _iconObjects;

	public PlayerEmotion(string emotionId, IClassCharacter classCharacter, IBattleResourceMgr resourceMgr)
		: base(emotionId, classCharacter)
	{
		_resourceMgr = resourceMgr;
	}

	public VfxBase LoadResource()
	{
		if (_iconObjects != null)
		{
			return InstantVfx.Create(UnfocusAllIcons);
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(_resourceMgr.LoadShardObject("UI/Battle/EmotionIcon", isAddUIContainer: false));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			GameObject sharedObject = _resourceMgr.GetSharedObject("UI/Battle/EmotionIcon");
			List<GameObject> list = new List<GameObject>();
			_iconObjects = new GameObject[7];
			for (int i = 0; i < 7; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(sharedObject);
				gameObject.SetActive(value: true);
				gameObject.transform.parent = _classCharacter.GameObject.transform;
				gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
				float f = (float)Math.PI * (1f - (float)i / 6f);
				float x = Mathf.Cos(f) * 240f;
				float y = Mathf.Sin(f) * 240f;
				gameObject.transform.localPosition = new Vector3(x, y, -35f);
				_iconObjects[i] = gameObject;
				list.Add(gameObject.transform.Find("Icon").gameObject);
				SetIconFocus(_iconObjects[i], focus: false);
			}
			UIManager.GetInstance().AttachAtlas(list);
		}));
		return sequentialVfxPlayer;
	}

	public VfxBase ShowButtons()
	{
		if (_iconObjects == null || _iconObjects[0].activeSelf)
		{
			return NullVfx.GetInstance();
		}
		_isIconShowReserve = true;
		return InstantVfx.Create(delegate
		{
			if (_isIconShowReserve)
			{
				for (int i = 0; i < _iconObjects.Length; i++)
				{
					TweenAlpha.Begin(_iconObjects[i], 0f, 1f);
					_iconObjects[i].SetActive(value: true);
				}
			}
			_isIconShowReserve = false;
		});
	}

	public VfxBase HideButtons()
	{
		return SequentialVfxPlayer.Create(new WaitFlagChangeVfx(() => !_isIconShowReserve), InstantVfx.Create(delegate
		{
			if (_iconObjects != null)
			{
				for (int i = 0; i < _iconObjects.Length; i++)
				{
					_iconObjects[i].SetActive(value: false);
				}
			}
			_isActiveHideTween = false;
		}));
	}

	public VfxBase HideButtons(GameObject iconObject)
	{
		return SequentialVfxPlayer.Create(new WaitFlagChangeVfx(() => !_isIconShowReserve), InstantVfx.Create(delegate
		{
			if (_iconObjects != null)
			{
				for (int i = 0; i < _iconObjects.Length; i++)
				{
					if (iconObject != _iconObjects[i])
					{
						_iconObjects[i].SetActive(value: false);
					}
				}
			}
		}), InstantVfx.Create(delegate
		{
			_isActiveHideTween = true;
			TweenAlpha tweenAlpha = TweenAlpha.Begin(iconObject, 0.3f, 0f);
			tweenAlpha.delay = 1.2f;
			tweenAlpha.onFinished.Clear();
			tweenAlpha.onFinished.Add(new EventDelegate(delegate
			{
				iconObject.SetActive(value: false);
				_isActiveHideTween = false;
			}));
		}));
	}

	public void CancelShowButtons()
	{
		_isIconShowReserve = false;
	}

	public void AddPlayCount()
	{
		_playCount++;
	}

	public void ResetPlayCount()
	{
		_playCount = 0;
	}

	public void FocusIcon(GameObject go)
	{
		if (!(go == _currentFocusIconObject))
		{
			_currentFocusIconObject = go;
			UnfocusAllIcons();
			SetIconFocus(go, focus: true);
		}
	}

	private void SetIconFocus(GameObject focusIconObject, bool focus)
	{
		if (!(focusIconObject == null))
		{
			string[] array = (focus ? FOCUS_ICON_NAMES : UNFOCUS_ICON_NAMES);
			int iconIndex = GetIconIndex(focusIconObject);
			focusIconObject.transform.Find("Icon").GetComponent<UISprite>().spriteName = array[iconIndex];
			focusIconObject.layer = 12;
		}
	}

	public void UnfocusAllIcons()
	{
		if (_iconObjects != null)
		{
			for (int i = 0; i < 7; i++)
			{
				SetIconFocus(_iconObjects[i], focus: false);
			}
		}
	}

	public VfxBase PlayEmotionFromIconObject(GameObject iconObject)
	{
		if (++_playCount > 3)
		{
			/* Pre-Phase-5b: PlayerBattleView.ShowAlert UI-only */
			return NullVfx.GetInstance();
		}
		ClassCharaPrm.EmotionType emoteType = IndexToEmotionType(GetIconIndex(iconObject));
		return PlayEmotion(emoteType, 1.5f);
	}

	public string GetVoiceTextFromIconObject(GameObject iconObject)
	{
		ClassCharaPrm.EmotionType key = IndexToEmotionType(GetIconIndex(iconObject));
		Dictionary<ClassCharaPrm.EmotionType, Wizard.Emotion> emotionDataBySkinId = new(); // Pre-Phase-5b: emotion data headless-empty
		if (emotionDataBySkinId.ContainsKey(key))
		{
			return emotionDataBySkinId[key].GetText(IsSkinEvolved());
		}
		return string.Empty;
	}

	private int GetIconIndex(GameObject iconObject)
	{
		for (int i = 0; i < _iconObjects.Length; i++)
		{
			if (_iconObjects[i] == iconObject)
			{
				return i;
			}
		}
		return -1;
	}

	public static ClassCharaPrm.EmotionType IndexToEmotionType(int index)
	{
		return index switch
		{
			0 => ClassCharaPrm.EmotionType.GREET, 
			1 => ClassCharaPrm.EmotionType.THANK, 
			2 => ClassCharaPrm.EmotionType.APOLOGY, 
			3 => ClassCharaPrm.EmotionType.PRAISE, 
			4 => ClassCharaPrm.EmotionType.SURPRISE, 
			5 => ClassCharaPrm.EmotionType.CONFUSE, 
			6 => ClassCharaPrm.EmotionType.PROVOCATION, 
			_ => ClassCharaPrm.EmotionType.GREET, 
		};
	}

	public bool IsContainsEmotionType(ClassCharaPrm.EmotionType type)
	{
		return new Dictionary<ClassCharaPrm.EmotionType, Wizard.Emotion>() // Pre-Phase-5b: emotion data headless-empty
			.ContainsKey(type);
	}

	public void DebugLogNotHiddenEmoteButton(TouchControl touchControl, VfxMgr emotionVfxMgr, SequentialVfxPlayer currentVfx)
	{
		if (_iconObjects == null || _iconObjects.Length == 0)
		{
			if (!_debugLogLimitter)
			{
				LocalLog.AccumulateTraceLog("#722006(0)");
				_debugLogLimitter = true;
			}
		}
		else
		{
			if (touchControl != null && touchControl.HasTouchProcessor && touchControl._touchProcessor is EmotionTouchProcessor)
			{
				return;
			}
			List<VfxBase> vfxList = emotionVfxMgr.GetVfxList<ParallelVfxPlayer>();
			for (int i = 0; i < vfxList.Count; i++)
			{
				List<VfxBase> vfxList2 = (vfxList[i] as ParallelVfxPlayer).GetVfxList();
				for (int j = 0; j < vfxList2.Count; j++)
				{
					if (!(vfxList2[j] is SequentialVfxPlayer))
					{
						continue;
					}
					SequentialVfxPlayer sequentialVfxPlayer = vfxList2[j] as SequentialVfxPlayer;
					if (sequentialVfxPlayer == currentVfx || sequentialVfxPlayer.IsEnd)
					{
						continue;
					}
				}
			}
			if (_isActiveHideTween)
			{
				return;
			}
			bool flag = false;
			for (int l = 0; l < _iconObjects.Length; l++)
			{
				if (_iconObjects[l].activeSelf)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (!_debugLogLimitter)
				{
					if (_isIconShowReserve)
					{
						LocalLog.AccumulateTraceLog("#722006(1)");
					}
					else
					{
						LocalLog.AccumulateTraceLog("#722006(2)");
					}
					_debugLogLimitter = true;
				}
			}
			else
			{
				_debugLogLimitter = false;
			}
		}
	}
}
