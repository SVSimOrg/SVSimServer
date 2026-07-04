using System.Collections;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class SlideObjectReceiveControl
{
	private NetworkBattleManagerBase _networkBattleMgr;

	private GameObject _slideStartObject;

	private GameObject _slideEndObject;

	private NetworkBattleSender.SLIDE_OBJECT_TYPE _slideObjectType;

	private IEnumerator _slideCoroutine;

	public SlideObjectReceiveControl(NetworkBattleManagerBase networkBattleMgr)
	{
		_networkBattleMgr = networkBattleMgr;
	}

	public void SlideObjectReceiveAction(NetworkBattleReceiver.ReceiveData receivedData)
	{
		bool isSelf = receivedData.isSelf;
		switch (receivedData._slideObjectType)
		{
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Cancel:
			CancelSlide();
			break;
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Attack:
		{
			BattlePlayerBase battlePlayer2 = _networkBattleMgr.GetBattlePlayer(isSelf);
			BattlePlayerBase battlePlayer3 = _networkBattleMgr.GetBattlePlayer(!isSelf);
			BattleCardBase indexToCardBase2 = NetworkBattleGenericTool.GetIndexToCardBase(_networkBattleMgr, battlePlayer2, receivedData.idx);
			BattleCardBase indexToCardBase3 = NetworkBattleGenericTool.GetIndexToCardBase(_networkBattleMgr, battlePlayer3, receivedData._selectedCardIndex);
			if (_slideStartObject != indexToCardBase2.BattleCardView.GameObject)
			{
				IBattleCardView battleCardView = indexToCardBase2.BattleCardView;
				_networkBattleMgr.VfxMgr.RegisterImmediateVfx(SequentialVfxPlayer.Create(NullVfx.GetInstance(), NullVfx.GetInstance()));
				_networkBattleMgr.VfxMgr.RegisterSequentialVfx(WaitVfx.Create(0.5f));
			}
			StartSlide(indexToCardBase2.BattleCardView.GameObject, indexToCardBase3.BattleCardView.GameObject, receivedData._slideObjectType, isSelf, isEvol: false);
			break;
		}
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Evolve:
		{
			BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(isSelf);
			BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_networkBattleMgr, battlePlayer, receivedData._selectedCardIndex);
			StartSlide(battlePlayer.BattleView.EpIcon, indexToCardBase.BattleCardView.GameObject, receivedData._slideObjectType, isSelf, isEvol: true);
			break;
		}
		default:
			Debug.LogError("Invalid Slide Object Type: " + receivedData._slideObjectType);
			break;
		}
	}

	public void CancelSlide()
	{
		if (!(_slideStartObject == null))
		{
			_networkBattleMgr.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			_networkBattleMgr.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			_slideStartObject = null;
			BattleCoroutine.GetInstance().StopCoroutine(_slideCoroutine);
			_slideCoroutine = null;
			_networkBattleMgr.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			_networkBattleMgr.GameMgr.GetEffectMgr().Stop(GetSlideLockOnEffect());
		}
	}

	private void StartSlide(GameObject startObject, GameObject endObject, NetworkBattleSender.SLIDE_OBJECT_TYPE slideObjectType, bool isTargettingEnemy, bool isEvol)
	{
		if (_slideStartObject != null && _slideStartObject != startObject)
		{
			CancelSlide();
		}
		_slideEndObject = endObject;
		if (_slideStartObject == null)
		{
			_slideStartObject = startObject;
			_slideObjectType = slideObjectType;
			_slideCoroutine = SlideToTarget();
			BattlePlayerBase battlePlayer = _networkBattleMgr.GetBattlePlayer(isTargettingEnemy);
			if (isEvol)
			{
				_networkBattleMgr.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			}
			_networkBattleMgr.BattlePlayer.PlayerBattleView.DragArrowStart(_networkBattleMgr, startObject, _networkBattleMgr.AttackArrowHead, isTargettingEnemy);
			BattleCoroutine.GetInstance().StartCoroutine(_slideCoroutine);
		}
		EffectMgr.EffectType slideLockOnEffect = GetSlideLockOnEffect();
		_networkBattleMgr.GameMgr.GetEffectMgr().Stop(slideLockOnEffect);
		_networkBattleMgr.GameMgr.GetEffectMgr().Start(slideLockOnEffect, endObject.transform.position, endObject);
	}

	private IEnumerator SlideToTarget()
	{
		while (_slideStartObject != null)
		{
			_networkBattleMgr.BattlePlayer.PlayerBattleView.DragArrow(_networkBattleMgr, _networkBattleMgr.AttackArrowHead, _slideEndObject.transform.position);
			yield return null;
		}
	}

	private EffectMgr.EffectType GetSlideLockOnEffect()
	{
		switch (_slideObjectType)
		{
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Attack:
			return EffectMgr.EffectType.CMN_CARD_TARGET_1;
		case NetworkBattleSender.SLIDE_OBJECT_TYPE.Evolve:
			return EffectMgr.EffectType.CMN_CARD_TARGET_2;
		default:
			Debug.LogError("Invalid Slide Object Type: " + _slideObjectType);
			return EffectMgr.EffectType.CMN_CARD_TARGET_1;
		}
	}
}
