using UnityEngine;
using Wizard.Battle.View.Vfx;

public interface ITurnPanelControl
{
	GameObject GameObject { get; }

	void Initialize(bool isEvoEnableP = true, bool isEvoEnableE = true);

	void StartUI(int turn, int evo, bool isP);

	VfxBase LoadResource();
}
