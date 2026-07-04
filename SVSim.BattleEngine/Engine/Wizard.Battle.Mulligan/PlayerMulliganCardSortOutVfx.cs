using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public class PlayerMulliganCardSortOutVfx : SequentialVfxPlayer
{
	public PlayerMulliganCardSortOutVfx(GameObject keepZone, IList<BattleCardBase> firstDraws, MulliganInfoControl mulliganUI)
	{
		Register(ShiftParentVfx(keepZone, firstDraws));
		Register(SortOutCardVfx(keepZone, firstDraws, mulliganUI));
	}

	private VfxBase ShiftParentVfx(GameObject keepZone, IList<BattleCardBase> firstDraws)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		int count = firstDraws.Count;
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = firstDraws[i].BattleCardView.GameObject;
				gameObject.SetActive(value: false);
				gameObject.transform.parent = keepZone.transform;
				MotionUtils.SetLayerAll(gameObject, 10);
				gameObject.SetActive(value: true);
			}
		}));
		return sequentialVfxPlayer;
	}

	private VfxBase SortOutCardVfx(GameObject keepZone, IList<BattleCardBase> firstDraws, MulliganInfoControl mulliganUI)
	{
		LocalLog.AccumulateLastTraceLog("SortOutCardVfx");
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int count = firstDraws.Count;
		for (int i = 0; i < count; i++)
		{
			parallelVfxPlayer.Register(MulliganViewBase.MoveCardToMulliganZone(firstDraws[i], keepZone, i, mulliganUI, isAbandon: false));
		}
		if (false /* Pre-Phase-5b: IsRecovery guard collapsed headless */)
		{
			return parallelVfxPlayer;
		}
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{

		}));
		return parallelVfxPlayer;
	}
}
