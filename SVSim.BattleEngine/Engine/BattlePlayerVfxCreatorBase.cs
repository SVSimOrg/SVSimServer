using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class BattlePlayerVfxCreatorBase : IBattlePlayerVfxCreator
{
	public BattlePlayerVfxCreatorBase(IBattlePlayerView battleView)
	{
	}

	public VfxBase CreateUsePp(int pp, int maxPp, Vector3 labelPosition, bool newReplayMoveTurn)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase CreateUseBp(int bp, int deltaBp, Func<Vector3> getPosition, bool isVariableCost, bool isSelf)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase CreateUpdateEp(int evolCount, int evolveWaitTurnCount)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase CreateCardDraw(IEnumerable<BattleCardBase> cards, bool isOpenDrawSkill)
	{
		return NullVfx.GetInstance();
	}
}
