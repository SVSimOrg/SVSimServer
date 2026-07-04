using System;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public interface IEmotion
{
	Vector3 LeaderPosition { get; }

	bool Enable { get; set; }

	event Func<ClassCharaPrm.EmotionType, VfxBase> OnPlay;

	VfxBase PlayEmotion(ClassCharaPrm.EmotionType emoteType, float hideTextTime);

	VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text);

	VfxBase ReceiveOpponentEmotion(ClassCharaPrm.EmotionType emotionType);
}
