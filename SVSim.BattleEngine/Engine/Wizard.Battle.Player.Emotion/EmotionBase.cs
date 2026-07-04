using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public abstract class EmotionBase : IEmotion
{

	protected readonly string _emotionId;

	protected readonly IClassCharacter _classCharacter;

	public Vector3 LeaderPosition
	{
		get
		{
			if (_classCharacter.GameObject == null)
			{
				return Vector3.zero;
			}
			return _classCharacter.GameObject.transform.position;
		}
	}

	public bool Enable { get; set; }

	public event Func<ClassCharaPrm.EmotionType, VfxBase> OnPlay;

	protected EmotionBase(string emotionId, IClassCharacter classCharacter)
	{
		Enable = true;
		_emotionId = emotionId;
		_classCharacter = classCharacter;
	}

	public VfxBase PlayEmotion(ClassCharaPrm.EmotionType emoteType, float hideTextTime)
	{
		if (!Enable)
		{
			return NullVfx.GetInstance();
		}
		// Pre-Phase-5b: emotion data headless-empty; branch dead
		return NullVfx.GetInstance();
	}

	public virtual VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text)
	{
		return PlayEmotion(motionType, faceType, voiceId, text, 1.5f);
	}

	public bool IsSkinEvolved()
	{
		// Pre-Phase-5b: no mgr in scope; skin-evolved gate defaults to false
		return false;
	}

	public virtual VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text, float hideTextTime, bool forcePlay = false)
	{
		if (!Enable)
		{
			return NullVfx.GetInstance();
		}
		if (IsSkinEvolved())
		{
			switch (motionType)
			{
			case ClassCharaPrm.MotionType.extra_2:
				motionType = ClassCharaPrm.MotionType.z_extra_2;
				break;
			case ClassCharaPrm.MotionType.damage:
				motionType = ClassCharaPrm.MotionType.z_damage;
				break;
			case ClassCharaPrm.MotionType.greet:
				motionType = ClassCharaPrm.MotionType.z_greet;
				break;
			case ClassCharaPrm.MotionType.idle:
				motionType = ClassCharaPrm.MotionType.z_idle;
				break;
			case ClassCharaPrm.MotionType.negative:
				motionType = ClassCharaPrm.MotionType.z_negative;
				break;
			case ClassCharaPrm.MotionType.negative_2:
				motionType = ClassCharaPrm.MotionType.z_negative_2;
				break;
			case ClassCharaPrm.MotionType.negative_2_a:
				motionType = ClassCharaPrm.MotionType.z_negative_2_a;
				break;
			case ClassCharaPrm.MotionType.positive:
				motionType = ClassCharaPrm.MotionType.z_positive;
				break;
			case ClassCharaPrm.MotionType.positive_2:
				motionType = ClassCharaPrm.MotionType.z_positive_2;
				break;
			case ClassCharaPrm.MotionType.shock:
				motionType = ClassCharaPrm.MotionType.z_shock;
				break;
			case ClassCharaPrm.MotionType.think:
				motionType = ClassCharaPrm.MotionType.z_think;
				break;
			}
		}
		if (text == "NONE")
		{
			return SequentialVfxPlayer.Create(_classCharacter.CreateLoadVoiceResource(voiceId), InstantVfx.Create(delegate
			{
				_classCharacter.PlayMotion(motionType);
				_classCharacter.ChangeFace(faceType);
			}), _classCharacter.PlayVoice(voiceId, forcePlay), WaitVfx.Create(hideTextTime));
		}
		VfxBase vfxBase = NullVfx.GetInstance();
		VfxBase vfxBase2 = NullVfx.GetInstance();
		if (text != string.Empty)
		{
			vfxBase = _classCharacter.ShowVoiceMessage(text);
			vfxBase2 = _classCharacter.HideVoiceMessage();
		}
		return SequentialVfxPlayer.Create(_classCharacter.CreateLoadVoiceResource(voiceId), InstantVfx.Create(delegate
		{
			_classCharacter.PlayMotion(motionType);
			_classCharacter.ChangeFace(faceType);
		}), _classCharacter.PlayVoice(voiceId, forcePlay), vfxBase, (hideTextTime > 0f) ? ((VfxBase)WaitVfx.Create(hideTextTime)) : ((VfxBase)new OpeningVfx.WaitVoiceEndVfx()), vfxBase2);
	}

	public virtual VfxBase ReceiveOpponentEmotion(ClassCharaPrm.EmotionType emotionType)
	{
		return NullVfx.GetInstance();
	}
}
