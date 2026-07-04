using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Mulligan;

public class MulliganOperateControl
{
	public enum STATE
	{
		WAIT,
		CARD_SELECTED,
		EXIT,
		NONE
	}

	private STATE _state;

	private InputMgr _inputManager;

	private PlayerMulliganCtrl _playerMulliganCtrl;

	protected PlayerMulliganView _mulliganView;

	protected IList<BattleCardBase> PlayerFirstDrawCards;

	private BattleCardBase _touchingCard;

	private BattleCardBase _showDetailCard;

	private float _timeSamePosition;

	private Vector2 _idlePosition;

	private BattleCardBase _pressedCard;

	public STATE State => _state;

	public MulliganOperateControl(PlayerMulliganCtrl mulliganCtrl)
	{
		_inputManager = null; // Pre-Phase-5b: no InputMgr headless
		_state = STATE.WAIT;
		_playerMulliganCtrl = mulliganCtrl;
		_mulliganView = mulliganCtrl.GetPlayerMulliganView();
		PlayerFirstDrawCards = mulliganCtrl.GetFirstDrawList();
		_mulliganView.MulliganInfo.OnTimeUp += ReturnOnMoveCardWhenMulliganPhaseEnd;
	}

	public VfxBase Update()
	{
		return _state switch
		{
			STATE.WAIT => Wait(), 
			STATE.CARD_SELECTED => CardSelected(), 
			_ => Reset(), 
		};
	}

	private VfxBase Wait()
	{
		if (BattleManagerBase.UseCustomMouse)
		{
			if (_inputManager.IsDown())
			{
				if ((_pressedCard = CheckCardCollision()) == null)
				{
					CheckDetailPanelCollision();
				}
			}
			else if (_inputManager.IsNone() || _inputManager.IsUp())
			{
				_pressedCard = null;
			}
			if (_inputManager.IsDown())
			{
				_showDetailCard = null;
			}
			if (_inputManager.IsClick() || (_inputManager.IsDownMoved() && _pressedCard != null))
			{
				if (CanMoveCard())
				{
					CheckSelectCard();
				}
			}
			else if (UseChangeShortcut())
			{
				if (false /* Pre-Phase-5b: MulliganOperateControl is UI-only headless */)
				{
					return NullVfx.GetInstance();
				}
				BattleCardBase battleCardBase = CheckCardCollision();
				if (_touchingCard == null && battleCardBase != null && !battleCardBase.IsOnDraw && !IsOnMoveFirstDrawCards())
				{
					if (_showDetailCard != null)
					{
						_mulliganView.ShutDownCardDetail();
						_showDetailCard = null;
						_idlePosition = _inputManager.GetPos();
						_timeSamePosition = 0f;
					}
					_touchingCard = battleCardBase;
					return ChangeCardSide();
				}
			}
			else if (UseDetailShortcut())
			{
				BattleCardBase battleCardBase2 = CheckCardCollision();
				if (battleCardBase2 != null && !battleCardBase2.IsOnMove && !battleCardBase2.IsOnDraw && battleCardBase2 != _showDetailCard)
				{
					_showDetailCard = battleCardBase2;
					_mulliganView.ShowCardDetail(_showDetailCard);
				}
			}
			else if (false /* Pre-Phase-5b: MulliganOperateControl is UI-only headless */)
			{
				RaycastHit[] hits = _mulliganView.ConvertMousePositionToFrontUIRaycastHits(Input.mousePosition);
				if (!IsTouchingDetail(hits))
				{
					if (_idlePosition == _inputManager.GetPos())
					{
						_timeSamePosition += Time.deltaTime;
						if (_timeSamePosition > 0f && _showDetailCard == null)
						{
							BattleCardBase battleCardBase3 = CheckCardCollision();
							if (battleCardBase3 != null && !battleCardBase3.IsOnMove && !battleCardBase3.IsOnDraw)
							{
								_showDetailCard = battleCardBase3;
								_mulliganView.ShowCardDetail(_showDetailCard);
							}
						}
					}
					else if (_showDetailCard != null)
					{
						BattleCardBase battleCardBase4 = CheckCardCollision();
						if (battleCardBase4 != _showDetailCard && battleCardBase4 != null && !battleCardBase4.IsOnDraw)
						{
							_showDetailCard = null;
							_mulliganView.ShutDownCardDetail();
							_idlePosition = _inputManager.GetPos();
							_timeSamePosition = 0f;
						}
					}
					else
					{
						_idlePosition = _inputManager.GetPos();
						_timeSamePosition = 0f;
					}
				}
			}
			return NullVfx.GetInstance();
		}
		if (_inputManager.IsDown())
		{
			CheckSelectCard();
		}
		return NullVfx.GetInstance();
	}

	private void CheckSelectCard()
	{
		if (_touchingCard != null)
		{
			return;
		}
		BattleCardBase battleCardBase = CheckCardCollision();
		if (battleCardBase != null)
		{
			if (battleCardBase.IsOnMove || battleCardBase.IsOnDraw)
			{
				return;
			}
			RaycastHit[] hits = _mulliganView.ConvertMousePositionToFrontUIRaycastHits(Input.mousePosition);
			if (!IsTouchingDetail(hits))
			{
				_touchingCard = battleCardBase;
				_state = STATE.CARD_SELECTED;
				if (BattleManagerBase.UseCustomMouse)
				{
					SetCardDragable();
				}
			}
		}
		else
		{
			CheckDetailPanelCollision();
		}
	}

	private void CheckDetailPanelCollision()
	{
		RaycastHit[] hits = _mulliganView.ConvertMousePositionToFrontUIRaycastHits(Input.mousePosition);
		if (!IsTouchingDetail(hits) && !UIManager.GetInstance().isOpenDialog())
		{
			_mulliganView.ShutDownCardDetail();
		}
	}

	private BattleCardBase CheckCardCollision()
	{
		RaycastHit[] hits = _mulliganView.ConvertMousePositionToRayCastHits(Input.mousePosition);
		if (IsTouchingSystemUI(hits))
		{
			return null;
		}
		return GetTouchedCardFromRaycast(hits);
	}

	private VfxBase CardSelected()
	{
		if (UIManager.GetInstance().IsQuitDialog())
		{
			return PutDownCard();
		}
		Vector3 mousePosition = Input.mousePosition;
		if (BattleManagerBase.UseCustomMouse && UseChangeShortcutDoubleClick())
		{
			if (_touchingCard != null && !_touchingCard.IsOnDraw)
			{
				return ChangeCardSide();
			}
		}
		else if (BattleManagerBase.UseCustomMouse && UseDetailShortcutDoubleClick())
		{
			if (_touchingCard != null && !_touchingCard.IsOnDraw)
			{
				_mulliganView.ShowCardDetail(_touchingCard);
				return PutDownCard();
			}
		}
		else
		{
			if (IsWantToPutDownCard())
			{
				return PutDownCard();
			}
			if (CanMoveCard())
			{
				if (_touchingCard == null)
				{
					return NullVfx.GetInstance();
				}
				if (_touchingCard.IsOnDraw)
				{
					return NullVfx.GetInstance();
				}
				if (_touchingCard.IsOnMove)
				{
					MoveMulliganCard(mousePosition);
				}
				else if (_inputManager.IsOverDragDistanceMulligan())
				{
					SetCardDragable();
				}
				else if (BattleManagerBase.UseCustomMouse && UseChangeShortcut())
				{
					return ChangeCardSide();
				}
			}
		}
		return NullVfx.GetInstance();
	}

	protected virtual void MoveMulliganCard(Vector3 mousePosition)
	{
		Vector3 position = new Vector3(mousePosition.x, mousePosition.y, 1.5f);
		Vector3 worldPointInMulliganUICamera = _mulliganView.GetWorldPointInMulliganUICamera(position);
		_mulliganView.DragCard(_touchingCard, worldPointInMulliganUICamera);
	}

	private VfxBase Reset()
	{
		_state = STATE.WAIT;
		return NullVfx.GetInstance();
	}

	protected virtual VfxBase PutDownCard()
	{
		_state = STATE.EXIT;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (_touchingCard != null)
		{
			BattleCardBase touchingCard = _touchingCard;
			_touchingCard = null;
			if (touchingCard.IsOnMove)
			{
				bool isAtAbandonZone = _playerMulliganCtrl.AbandonList.Contains(touchingCard);
				int posIndex = PlayerFirstDrawCards.IndexOf(touchingCard);
				if (IsTouchingAbandonZone(Input.mousePosition, isAtAbandonZone))
				{
					sequentialVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(touchingCard, posIndex, isAbandon: true));
					_playerMulliganCtrl.RegisterAbandonCard(touchingCard);
				}
				else
				{
					sequentialVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(touchingCard, posIndex, isAbandon: false));
					_playerMulliganCtrl.TakeOutAbandonCard(touchingCard);
				}
			}
			else
			{
				_mulliganView.ShowCardDetail(touchingCard);
			}
		}
		return sequentialVfxPlayer;
	}

	public VfxBase ReturnOnMoveCardWhenMulliganPhaseEnd()
	{
		if (_touchingCard != null && _touchingCard.IsOnMove)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			if (_playerMulliganCtrl.AbandonList.Contains(_touchingCard))
			{
				_playerMulliganCtrl.AbandonList.Remove(_touchingCard);
			}
			int posIndex = PlayerFirstDrawCards.IndexOf(_touchingCard);
			sequentialVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(_touchingCard, posIndex, isAbandon: false));
			sequentialVfxPlayer.Register(WaitVfx.Create(0.3f));
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				_mulliganView.DragCardStop(_touchingCard);
			}));
			return sequentialVfxPlayer;
		}
		return NullVfx.GetInstance();
	}

	private BattleCardBase GetTouchedCardFromRaycast(RaycastHit[] hits)
	{
		int num = hits.Length;
		for (int i = 0; i < num; i++)
		{
			RaycastHit hit = hits[i];
			BattleCardBase hitCard = GetHitCard(hit);
			if (hitCard != null && IsCardDraggable(hitCard))
			{
				return hitCard;
			}
		}
		return null;
	}

	protected virtual bool IsCardDraggable(BattleCardBase hitCard)
	{
		return PlayerFirstDrawCards.Contains(hitCard);
	}

	public bool IsOnMoveFirstDrawCards()
	{
		return PlayerFirstDrawCards.Any((BattleCardBase c) => c.IsOnMove);
	}

	private BattleCardBase GetHitCard(RaycastHit hit)
	{
		if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("PlayerToken"))
		{
			GameObject gameObject = hit.collider.transform.parent.gameObject;
			if (gameObject.layer == 10)
			{
				BattleCardBase battleCard = gameObject.GetBattleCard();
				if (battleCard != null)
				{
					return battleCard;
				}
			}
		}
		return null;
	}

	private bool IsTouchingDetail(RaycastHit[] hits)
	{
		int num = hits.Length;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hits[i];
			if (raycastHit.collider.CompareTag("DetailPanel") && raycastHit.collider.gameObject.layer == 26)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTouchingSystemUI(RaycastHit[] hits)
	{
		int num = hits.Length;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hits[i];
			if (raycastHit.collider.gameObject.layer == 22)
			{
				return true;
			}
		}
		return false;
	}

	protected bool IsTouchingAbandonZone(Vector3 mousePosition, bool isAtAbandonZone)
	{
		if (isAtAbandonZone)
		{
			return !_mulliganView.MulliganInfo.IsLeavingAbandonZone(mousePosition);
		}
		return _mulliganView.MulliganInfo.IsLeavingKeepZone(mousePosition);
	}

	private bool IsWantToPutDownCard()
	{
		if (BattleManagerBase.UseCustomMouse)
		{
			if (!_inputManager.IsUp() && !_inputManager.IsDoubleClick())
			{
				return _inputManager.IsClick();
			}
			return true;
		}
		if (!_inputManager.IsUp())
		{
			return _inputManager.IsNone();
		}
		return true;
	}

	protected virtual bool CanMoveCard()
	{
		if (BattleManagerBase.UseCustomMouse)
		{
			return true;
		}
		return _inputManager.IsPress();
	}

	protected virtual VfxBase ChangeCardSide()
	{
		_state = STATE.EXIT;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattleCardBase touchingCard = _touchingCard;
		_touchingCard = null;
		bool num = _playerMulliganCtrl.AbandonList.Contains(touchingCard);
		int num2 = PlayerFirstDrawCards.IndexOf(touchingCard);
		_mulliganView.MulliganInfo.SetExchangeMarkPlayer(num2, on: false);
		if (!num)
		{
			sequentialVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(touchingCard, num2, isAbandon: true));
			_playerMulliganCtrl.RegisterAbandonCard(touchingCard);
		}
		else
		{
			sequentialVfxPlayer.Register(_mulliganView.MoveCardToStaticPosition(touchingCard, num2, isAbandon: false));
			_playerMulliganCtrl.TakeOutAbandonCard(touchingCard);
		}
		return sequentialVfxPlayer;
	}

	protected virtual void SetCardDragable()
	{
		int index = PlayerFirstDrawCards.IndexOf(_touchingCard);
		_mulliganView.MulliganInfo.SetExchangeMarkPlayer(index, on: false);
		_mulliganView.DragCardStart(_touchingCard);
		_mulliganView.ShutDownCardDetail();
	}

	// OptionSettingWindow removed (DEAD-COLD engine cleanup Task 13) — shortcut methods return false (headless default)
	private bool UseChangeShortcut() => false;

	private bool UseChangeShortcutDoubleClick() => false;

	private bool UseDetailShortcut() => false;

	private bool UseDetailShortcutDoubleClick() => false;
}
