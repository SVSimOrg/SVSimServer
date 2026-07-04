using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public class EnemyEmotionBase : EmotionBase
{
	public EnemyEmotionBase(string emotionId, IClassCharacter classCharacter)
		: base(emotionId, classCharacter)
	{
	}

	public override VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text)
	{
		if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE))
		{
			return NullVfx.GetInstance();
		}
		return base.PlayEmotion(motionType, faceType, voiceId, text);
	}

	public override VfxBase PlayEmotion(ClassCharaPrm.MotionType motionType, ClassCharaPrm.FaceType faceType, string voiceId, string text, float hideTextTime, bool forcePlay = false)
	{
		ClassCharaPrm.MotionType motionType2 = motionType;
		switch (motionType)
		{
		case ClassCharaPrm.MotionType.extra:
			motionType2 = ClassCharaPrm.MotionType.extra_2;
			break;
		case ClassCharaPrm.MotionType.extra_1_a:
			motionType2 = ClassCharaPrm.MotionType.extra_2_a;
			break;
		case ClassCharaPrm.MotionType.extra_1_b:
			motionType2 = ClassCharaPrm.MotionType.extra_2_b;
			break;
		case ClassCharaPrm.MotionType.extra_1_c:
			motionType2 = ClassCharaPrm.MotionType.extra_2_c;
			break;
		}
		if (forcePlay)
		{
			return base.PlayEmotion(motionType2, faceType, voiceId, text, hideTextTime, forcePlay);
		}
		if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE))
		{
			return NullVfx.GetInstance();
		}
		return base.PlayEmotion(motionType2, faceType, voiceId, text, hideTextTime, forcePlay);
	}
}
