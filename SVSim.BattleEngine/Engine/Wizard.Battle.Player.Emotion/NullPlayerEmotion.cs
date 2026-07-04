using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 21 of 22 methods unrun in baseline
//   Type: Wizard.Battle.Player.Emotion.NullPlayerEmotion
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Player.Emotion;

public class NullPlayerEmotion : IPlayerEmotion, IEmotion
{
	public Vector3 LeaderPosition => Vector3.zero;

	public IEnumerable<GameObject> IconObjects => null;

	public bool Enable { get; set; }

	public event Func<ClassCharaPrm.EmotionType, VfxBase> OnPlay;

	public VfxBase PlayEmotion(ClassCharaPrm.EmotionType emoteType, float hideTextTime)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ReceiveOpponentEmotion(ClassCharaPrm.EmotionType emoteType)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase LoadResource()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ShowButtons()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase HideButtons()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase HideButtons(GameObject iconObject)
	{
		return NullVfx.GetInstance();
	}

	public void CancelShowButtons()
	{
	}

	public void ResetPlayCount()
	{
	}

	public void AddPlayCount()
	{
	}

	public void FocusIcon(GameObject go)
	{
	}

	public void UnfocusAllIcons()
	{
	}

	public VfxBase PlayEmotionFromIconObject(GameObject iconObject)
	{
		return NullVfx.GetInstance();
	}

	public string GetVoiceTextFromIconObject(GameObject iconObject)
	{
		return string.Empty;
	}

	public bool IsContainsEmotionType(ClassCharaPrm.EmotionType type)
	{
		return false;
	}

	public void DebugLogNotHiddenEmoteButton(TouchControl touchControl, VfxMgr emotionVfxMgr, SequentialVfxPlayer currentVfx)
	{
	}
}
