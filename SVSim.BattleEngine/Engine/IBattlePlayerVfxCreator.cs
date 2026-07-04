using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;

public interface IBattlePlayerVfxCreator
{
	VfxBase CreateUsePp(int pp, int maxPp, Vector3 labelPosition, bool newReplayMoveTurn);

	VfxBase CreateUseBp(int bp, int deltaBp, Func<Vector3> getPosition, bool isVariableCost, bool isSelf);

	VfxBase CreateUpdateEp(int evolCount, int evolveWaitTurnCount);

	VfxBase CreateCardDraw(IEnumerable<BattleCardBase> cards, bool isOpenDrawSkill = false);
}
