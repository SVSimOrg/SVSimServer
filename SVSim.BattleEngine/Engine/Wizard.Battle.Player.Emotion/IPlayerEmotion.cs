using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Player.Emotion;

public interface IPlayerEmotion : IEmotion
{
	IEnumerable<GameObject> IconObjects { get; }

	VfxBase LoadResource();

	VfxBase ShowButtons();

	VfxBase HideButtons();

	VfxBase HideButtons(GameObject iconObject);

	void CancelShowButtons();

	void ResetPlayCount();

	void AddPlayCount();

	void FocusIcon(GameObject go);

	void UnfocusAllIcons();

	VfxBase PlayEmotionFromIconObject(GameObject iconObject);

	string GetVoiceTextFromIconObject(GameObject iconObject);

	bool IsContainsEmotionType(ClassCharaPrm.EmotionType type);

	void DebugLogNotHiddenEmoteButton(TouchControl touchControl, VfxMgr emotionVfxMgr, SequentialVfxPlayer currentVfx);
}
