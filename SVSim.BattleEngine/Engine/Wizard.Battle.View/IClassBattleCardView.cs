using Wizard.Battle.Player.ClassCharacter;

namespace Wizard.Battle.View;

public interface IClassBattleCardView : IBattleCardView
{
	IClassCharacter ClassCharacter { get; }

	void StartOutFrame();

	void StartIntoFrame();

	float GetCurrentClipTime();

	bool GetCurrentClipIsName(ClassCharaPrm.MotionType motionType);

	void ClearSpineObject();
}
