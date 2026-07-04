using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.ClassCharacter;

public interface IClassCharacter
{
	GameObject GameObject { get; }

	bool IsWaiting { get; }

	bool IsRecovery { get; }

	VfxBase CreateLoadResouceVfx();

	void PlayMotion(ClassCharaPrm.MotionType motionType);

	void ResetMotion();

	void ChangeFace(ClassCharaPrm.FaceType faceType);

	void SetAnimationEnable(bool enable);

	bool IsAnimationEnable();

	VfxBase CreateLoadVoiceResource(string voiceId);

	VfxBase PlayVoice(string voiceId, bool forcePlay = false);

	VfxBase PlaySkinEvolveSe(string skinId, string suffix);

	VfxBase ShowVoiceMessage(string text);

	VfxBase HideVoiceMessage();

	VfxBase SetWaiting(bool flag);

	VfxBase SetRecovery(bool flag);

	VfxBase ResetStatusInfo();

	VfxBase UpdateEnviromentMessage();

	ClassCharaPrm.MotionType GetMotion();

	bool IsNoEvolveShift();

	bool IsOpponentReverse();
}
