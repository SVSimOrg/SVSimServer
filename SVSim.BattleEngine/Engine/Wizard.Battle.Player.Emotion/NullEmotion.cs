using System;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public class NullEmotion : IEmotion
{
	public Vector3 LeaderPosition => Vector3.zero;

	public bool Enable { get; set; }

	public event Func<ClassCharaPrm.EmotionType, VfxBase> OnPlay;

	public VfxBase PlayEmotion(ClassCharaPrm.EmotionType etype, float hideTextTime)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ReceiveOpponentEmotion(ClassCharaPrm.EmotionType emotionType)
	{
		return NullVfx.GetInstance();
	}
}
