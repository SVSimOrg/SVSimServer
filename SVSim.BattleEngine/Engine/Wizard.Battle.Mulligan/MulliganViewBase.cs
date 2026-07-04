using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 4 of 9 methods unrun in baseline
//   Type: Wizard.Battle.Mulligan.MulliganViewBase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Mulligan;

public abstract class MulliganViewBase
{
	public static readonly Vector3 CARD_ROTATION = new Vector3(0f, 0f, 0f);

	public static readonly Vector3 CARD_ROTATION_OPPO = new Vector3(0f, 180f, 0f);

	protected MulliganInfoControl m_MlgUI;

	public MulliganInfoControl MulliganInfo => m_MlgUI;

	public MulliganViewBase(MulliganInfoControl mulliganInfo)
	{
		m_MlgUI = mulliganInfo;
	}

	public VfxBase UpdateOpponentMulliganStatusLabel(int Count)
	{
		return InstantVfx.Create(delegate
		{
			m_MlgUI.SetEnemyReady(Count);
		});
	}

	public VfxBase SortFirstDrawsToKeepZone(IList<BattleCardBase> firstDraws)
	{
		return new PlayerMulliganCardSortOutVfx(GetMulliganUIKeepZone(), firstDraws, m_MlgUI);
	}

	public static VfxBase MoveCardToMulliganZone(BattleCardBase target, GameObject mulliganZone, int posIndex, MulliganInfoControl mulliganUI, bool isAbandon)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			GameObject gameObject = target.BattleCardView.GameObject;
			target.BattleCardView.GameObject.transform.parent = mulliganZone.transform;
			Vector3 mulliganZoneCardPos = mulliganUI.GetMulliganZoneCardPos(posIndex, isAbandon, target.IsPlayer);
			Vector3 mulliganZoneCardScale = mulliganUI.GetMulliganZoneCardScale();
			if (false /* Pre-Phase-5b: IsRecovery guard headless-safe as false in view code */)
			{
				gameObject.transform.localScale = mulliganZoneCardScale;
			}
			else
			{
				iTween.ScaleTo(gameObject, iTween.Hash("scale", mulliganZoneCardScale, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
			}
			if (false /* Pre-Phase-5b: IsWatchBattle const-false */)
			{
				if (false /* Pre-Phase-5b: IsAdmin const-false */)
				{
					RotatePlayer(gameObject);
				}
				else if (false /* Pre-Phase-5b: no BattleView headless */)
				{
					RotateEnemy(gameObject);
				}
				else if (target.IsPlayer)
				{
					RotatePlayer(gameObject);
				}
				else
				{
					RotateEnemy(gameObject);
				}
			}
			else
			{
				RotatePlayer(gameObject);
			}
			if (false /* Pre-Phase-5b: IsRecovery guard headless-safe as false in view code */)
			{
				gameObject.transform.localPosition = mulliganZoneCardPos;
			}
			else
			{
				iTween.MoveTo(gameObject, iTween.Hash("position", mulliganZoneCardPos, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
			}
		}));
		return sequentialVfxPlayer;
	}

	private static void RotatePlayer(GameObject cardObj)
	{
		if (false /* Pre-Phase-5b: IsRecovery guard headless-safe as false in view code */)
		{
			cardObj.transform.localRotation = Quaternion.Euler(CARD_ROTATION);
			return;
		}
		iTween.RotateTo(cardObj, iTween.Hash("rotation", CARD_ROTATION, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
	}

	private static void RotateEnemy(GameObject cardObj)
	{
		if (false /* Pre-Phase-5b: IsRecovery guard headless-safe as false in view code */)
		{
			cardObj.transform.localRotation = Quaternion.Euler(CARD_ROTATION_OPPO);
			return;
		}
		iTween.RotateTo(cardObj, iTween.Hash("rotation", CARD_ROTATION_OPPO, "time", 0.3f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
	}

	public virtual SequentialVfxPlayer MoveCardToStaticPosition(BattleCardBase card, int posIndex, bool isAbandon)
	{
		GameObject mulliganZone = (isAbandon ? GetMulliganUIAbandonZone() : GetMulliganUIKeepZone());
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			card.SetOnMove(move: true);
		}));
		sequentialVfxPlayer.Register(MoveCardToMulliganZone(card, mulliganZone, posIndex, m_MlgUI, isAbandon));
		sequentialVfxPlayer.Register(WaitVfx.Create(0.3f));
		return sequentialVfxPlayer;
	}

	protected abstract GameObject GetMulliganUIKeepZone();

	protected abstract GameObject GetMulliganUIAbandonZone();

	public abstract void HideMulliganUIAbandonZone();

	public void HideMulliganTitle()
	{
		m_MlgUI.HideMulliganTitle();
	}
}
