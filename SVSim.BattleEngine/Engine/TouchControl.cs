using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.Touch;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class TouchControl
{
	public bool IsForceEnd;

	public bool IsDisconnect;

	protected BattleManagerBase _battleMgr;

	private BattleCamera _battleCamera;

	private BackGroundBase _backGround;

	protected VfxMgr _emotionVfxMgr;

	protected VfxMgr _predictionVfxMgr;

	protected Prediction _prediction;

	protected VfxMgr _classEffectVfxMgr;

	protected InputMgr _inputMgr;

	private IPlayerView _battlePlayerView;

	private IBattlePlayerView _battleEnemyView;

	public BattleCardBase _hitCard;

	private DetailPanelTouchProcessor _detailPanelTouchProcessor;

	private EvolutionTouchProcessor _evolutionTouchProcessor;

	private BattleCardBase _detailCardDisplay;

	private BattleCardBase _detailCardHover;

	private BattleCardBase _detailCardStart;

	private bool _isDetailCardChoiceBrave;

	private float _hoverTime;

	protected BattleCardBase _alertCard;

	private bool _isUsingEmote;

	private bool _isTapPlayerClass;

	private Vector2 _lastMousePos;

	private bool _evolutionClick;

	public static BattleCardBase KeepAlertCard;

	protected BattleCardBase _pressedCard;

	public bool IsProcessorStart { get; private set; }

	public bool IsProcessorUpdate { get; private set; }

	public bool IsProcessorEnd { get; private set; }

	public bool IsTouchCancel
	{
		get
		{
			if (!IsDisconnect && !IsForceEnd)
			{
				return _battleMgr.BattlePlayer.IsTimeOverTurnEndProcessing;
			}
			return true;
		}
	}

	public ITouchProcessor _touchProcessor { get; private set; }

	public bool HasTouchProcessor => _touchProcessor != null;

	protected BattlePlayer BattlePlayer => _battleMgr.BattlePlayer;

	protected BattleEnemy BattleEnemy => _battleMgr.BattleEnemy;

	public event Action OnEvolveFocus;

	public event Action OnEvolveUnfocus;

	public event Func<VfxBase> OnAfterEvolveDragSelect;

	public event Func<VfxBase> OnStartEvolveSkillTargetSelect;

	public TouchControl(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround)
	{
		_battleMgr = battleMgr;
		_battleCamera = battleCamera;
		_backGround = backGround;
		_emotionVfxMgr = new VfxMgr();
		_predictionVfxMgr = new VfxMgr();
		_prediction = _battleMgr.Prediction;
		_classEffectVfxMgr = new VfxMgr();
		_inputMgr = _battleMgr.GameMgr.GetInputMgr();
		_battlePlayerView = battleMgr.BattlePlayer.PlayerBattleView;
		_battleEnemyView = battleMgr.BattleEnemy.BattleEnemyView;
	}

	public void Dispose()
	{
		_prediction = null;
		if (_classEffectVfxMgr != null)
		{
			_classEffectVfxMgr.Dispose();
			_classEffectVfxMgr = null;
		}
		if (_emotionVfxMgr != null)
		{
			_emotionVfxMgr.Dispose();
			_emotionVfxMgr = null;
		}
		if (_predictionVfxMgr != null)
		{
			_predictionVfxMgr.Dispose();
			_predictionVfxMgr = null;
		}
		if (_classEffectVfxMgr != null)
		{
			_classEffectVfxMgr.Dispose();
			_classEffectVfxMgr = null;
		}
		KeepAlertCard = null;
	}

	public VfxBase Update(float dt)
	{
		if (UIManager.GetInstance().IsQuitDialog())
		{
			return NullVfx.GetInstance();
		}
		if (_battleMgr.IsRecovery)
		{
			return NullVfx.GetInstance();
		}
		if (!IsFeasibleAttack())
		{
			if (_detailPanelTouchProcessor != null)
			{
				_detailPanelTouchProcessor.StopAttackTarget();
			}
			if (_battlePlayerView != null)
			{
				StopDraggingArrow();
			}
		}
		if (!IsFeasibleEvol() && _evolutionTouchProcessor != null)
		{
			_evolutionTouchProcessor.SetStopSelectFlag();
		}
		VfxBase instance = NullVfx.GetInstance();
		if (HasTouchProcessor)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			parallelVfxPlayer.Register(_touchProcessor.Update(dt, _battleCamera.Camera));
			IsProcessorUpdate = true;
			if ((_touchProcessor.CheckIsEnd() || IsTouchCancel) && IsProcessorStart && IsProcessorUpdate)
			{
				VfxWith<ITouchProcessor> vfxWith = _touchProcessor.End();
				IsProcessorEnd = true;
				_touchProcessor = vfxWith.Value;
				IsProcessorStart = false;
				IsProcessorUpdate = false;
				parallelVfxPlayer.Register(vfxWith.Vfx);
				if (HasTouchProcessor)
				{
					VfxBase vfx = _touchProcessor.Start();
					IsProcessorStart = true;
					parallelVfxPlayer.Register(vfx);
				}
			}
			instance = parallelVfxPlayer;
		}
		else
		{
			instance = Wait();
		}
		UpdateVfxMgrs(dt);
		_lastMousePos = Input.mousePosition;
		return instance;
	}

	private void UpdateVfxMgrs(float dt)
	{
		_emotionVfxMgr.Update(dt);
		_classEffectVfxMgr.Update(dt);
		_predictionVfxMgr.Update(dt);
		_prediction.Update(dt);
	}

	private VfxBase Wait()
	{
		if (_battleMgr.IsBattleEnd)
		{
			return NullVfx.GetInstance();
		}
		bool flag = _battlePlayerView.HandControl.IsHandStateFocus() || BattleEnemy.BattleEnemyView.HandControl.IsHandStateFocus();
		if (_inputMgr.IsNone() || _inputMgr.IsUp())
		{
			_evolutionClick = false;
			_isTapPlayerClass = false;
		}
		if (_battlePlayerView == null || _battlePlayerView.HandView == null || _battlePlayerView.HandControl == null)
		{
			return NullVfx.GetInstance();
		}
		if (_battlePlayerView.IsEvolutionStart && _battlePlayerView.IsMenuOpen)
		{
			_battlePlayerView.IsEvolutionStart = false;
			_battlePlayerView.ShowTurnEndButton();
		}
		bool flag2 = _inputMgr.IsDown();
		if (!BattleManagerBase.UseCustomMouse && !flag2 && !_isTapPlayerClass)
		{
			return NullVfx.GetInstance();
		}
		if (BattleManagerBase.UseCustomMouse && !_battleMgr.HasFocus && !_isTapPlayerClass)
		{
			return NullVfx.GetInstance();
		}
		Vector3 mousePosition = Input.mousePosition;
		Ray ray = _battleCamera.Camera.ScreenPointToRay(mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity);
		if (_isTapPlayerClass && !flag && IsFeasibleEmote())
		{
			EmotionTouchProcessor touchProcessor = CreateEmotionTouchProcessor(_battleMgr, _inputMgr, _emotionVfxMgr);
			_isTapPlayerClass = false;
			if (BattleManagerBase.UseCustomMouse)
			{
				_isUsingEmote = true;
			}
			return RegisterTouchProcessor(touchProcessor);
		}
		_isTapPlayerClass = false;
		Ray ray2 = DetailMgr.GetCamera().ScreenPointToRay(mousePosition);
		RaycastHit[] array = null;
		if (!_battlePlayerView.IsTouchable())
		{
			return NullVfx.GetInstance();
		}
		UIManager instance = UIManager.GetInstance();
		if (instance.isOpenDialog() || instance.isFading())
		{
			return NullVfx.GetInstance();
		}
		if (_battleCamera.Camera == null)
		{
			return NullVfx.GetInstance();
		}
		_battlePlayerView.LockOnEffectOff();
		if (!flag)
		{
			if (BattleManagerBase.UseCustomMouse && (_inputMgr.IsNone() || _inputMgr.IsDown()))
			{
				if (_battlePlayerView.IsDetailOn() && array == null)
				{
					array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
				}
				if (TryZoomHand(hits, array))
				{
					return NullVfx.GetInstance();
				}
			}
			else if (!BattleManagerBase.UseCustomMouse && _inputMgr.IsDown())
			{
				if (_battlePlayerView.IsDetailOn() && array == null)
				{
					array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
				}
				flag = TryZoomHand(hits, array);
			}
		}
		else if (_battleMgr.GameMgr.IsAdmin && _inputMgr.IsDown())
		{
			if (_battlePlayerView.IsDetailOn() && array == null)
			{
				array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
			}
			flag |= TryZoomHand(hits, array);
		}
		if (BattleManagerBase.UseCustomMouse)
		{
			BattleCardBase hitCard = TouchCard(hits);
			if (_inputMgr.IsDown())
			{
				_pressedCard = hitCard;
			}
			else if (_inputMgr.IsNone() || _inputMgr.IsUp())
			{
				_pressedCard = null;
			}
			bool flag3 = hitCard != null && !hitCard.IsOnDraw && !hitCard.IsClass;
			if (flag3 && hitCard.IsInHand && hitCard.IsPlayer && UsePlayShortcut())
			{
				if (BattlePlayer.IsSelfTurn && flag && !_battlePlayerView.IsSelecting && !hitCard.IsOnMove && hitCard.BattleCardView.GameObject.GetComponent<iTween>() == null)
				{
					if (_battlePlayerView.IsDetailOn() && array == null)
					{
						array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
					}
					if (!_battlePlayerView.IsDetailOn() || !array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
					{
						_hitCard = hitCard;
						if (WillPlayCardFromHand(showAlert: true))
						{
							_battlePlayerView.MoveCardCancel(_hitCard, _hitCard.BattleCardView.GameObject.transform.localPosition, _hitCard.BattleCardView.GameObject.transform.localRotation, IsPress: false);
							_battleMgr.BattleUIContainer.DisableMenu();
							SetCardProcessor setCardProcessor = CreateSetCardProcessor(_hitCard);
							if (setCardProcessor == null)
							{
								return NullVfx.GetInstance();
							}
							EmitHandUtility.SendSelectObject(_battleMgr, null);
							return RegisterTouchProcessor(setCardProcessor);
						}
						return NullVfx.GetInstance();
					}
					return NullVfx.GetInstance();
				}
			}
			else
			{
				if (flag3 && hitCard.IsInplay && hitCard.IsPlayer && UseEvolutionShortcut() && !_battlePlayerView.IsSelecting)
				{
					if (_battlePlayerView.IsDetailOn() && array == null)
					{
						array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
					}
					if (!_battlePlayerView.IsDetailOn() || !array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
					{
						if (_battleMgr.CanOpenEvolutionConfirmation(hitCard))
						{
							HideAlert();
							HideDetailPanel();
							if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.CONFIRM_EVOLVE))
							{
								_battleMgr.DetailMgr.DetailPanelControl._evolutionConfirmation.Show(BattlePlayer).onPushButton1 = delegate
								{
									_battleMgr.BattlePlayer.PlayerBattleView._isEvolutionSkillSelect = true;
									RegisterTouchProcessor(CreateEvolutionSimpleProcessor(hitCard));
								};
								return NullVfx.GetInstance();
							}
							_battleMgr.BattlePlayer.PlayerBattleView._isEvolutionSkillSelect = true;
							EvolutionSimpleProcessor touchProcessor2 = CreateEvolutionSimpleProcessor(hitCard);
							return RegisterTouchProcessor(touchProcessor2);
						}
						return NullVfx.GetInstance();
					}
					return NullVfx.GetInstance();
				}
				if (hitCard != null && UseDetailShortcut() && hitCard.IsHoverActionCard() && _detailCardStart != hitCard && (!_battlePlayerView.IsDetailOn() || _battlePlayerView.DetailOpenCard != hitCard))
				{
					if (_battlePlayerView.IsDetailOn() && array == null)
					{
						array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
					}
					if (!_battlePlayerView.IsDetailOn() || !array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
					{
						HideAlert();
						StartOpenHandDetail(hitCard, !hitCard.IsInHand && Input.mousePosition.x < (float)Screen.width / 2f);
					}
				}
				else if (_inputMgr.IsNone())
				{
					if (_battlePlayerView.IsDetailOn() && array == null)
					{
						array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
					}
					if (!_battlePlayerView.IsDetailOn() || !array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
					{
						checkHoverCard(hits, flag);
					}
				}
			}
		}
		if (_battlePlayerView.IsMenuOpen)
		{
			return NullVfx.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, float.PositiveInfinity))
		{
			if (flag2)
			{
				if (hitInfo.collider.CompareTag("CardHolder"))
				{
					return RegisterDeckTouchProcessor(_battlePlayerView);
				}
				if (hitInfo.collider.CompareTag("ECardHolder"))
				{
					return RegisterDeckTouchProcessor(_battleEnemyView);
				}
			}
			BattleCardBase battleCardBase = TouchCard(hits);
			if (_battlePlayerView.IsDetailOn() && flag2)
			{
				if (array == null)
				{
					array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
				}
				if (array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
				{
					return parallelVfxPlayer;
				}
				if (battleCardBase == null && !hitInfo.collider.CompareTag("ClassBtn") && !CheckChoiceBraveButton(hits, "PlayerChoiceBraveButton") && !CheckChoiceBraveButton(hits, "EnemyChoiceBraveButton"))
				{
					ResetDetail();
					EmitHandUtility.SendSelectObject(_battleMgr, null);
					if (BattleManagerBase.UseCustomMouse && !array.Any((RaycastHit entry) => entry.collider.CompareTag("BattleUI")))
					{
						_battleMgr.VfxMgr.RegisterImmediateVfx(_battlePlayerView.HandUnfocus());

					}
					return parallelVfxPlayer;
				}
			}
			if (battleCardBase == null)
			{
				if (flag2 && !_battlePlayerView.IsDetailOn() && flag && !hitInfo.collider.CompareTag("BattleUI") && !hitInfo.collider.CompareTag("ClassBtn") && !CastRayWithBattleUI(mousePosition))
				{
					_battleMgr.VfxMgr.RegisterImmediateVfx(_battlePlayerView.HandUnfocus());
					_battleMgr.VfxMgr.RegisterImmediateVfx(_battleEnemyView.HandUnfocus());

				}
				if (flag2)
				{
					_evolutionClick = CheckEpPanel(hitInfo.collider);
				}
				if (CheckEpPanel(hitInfo.collider) && BattlePlayer.BattleView.IsTouchable() && IsFeasibleEvol() && (BattlePlayer.IsEvolve || BattlePlayer.IsExceptionEvolve) && BattlePlayer.IsSelfTurn && ((!BattleManagerBase.UseCustomMouse && flag2) | (BattleManagerBase.UseCustomMouse && (_inputMgr.IsClick() || (_inputMgr.IsDownMoved() && _evolutionClick)))))
				{
					_battleMgr.VfxMgr.RegisterImmediateVfx(RegisterEvolutionTouchProcessor());
					return NullVfx.GetInstance();
				}
				if (flag2)
				{
					if (CheckChoiceBraveButton(hits, "PlayerChoiceBraveButton"))
					{
						BattleCardBase battleCardBase2 = _battleMgr.BattlePlayer.Class;
						if (!_battleMgr.GameMgr.IsWatchBattle && BattlePlayer.CanChoiceBrave)
						{
							ResetDetail();

							_battlePlayerView.UpdateChoiceBraveActivatingEffect(isActivating: true);
							List<SkillBase> choiceSkills = battleCardBase2.Skills.Where((SkillBase s) => s is Skill_choice && s.OnWhenChoiceBrave != 0).ToList();
							parallelVfxPlayer.Register(RegisterTouchProcessor(new ChoiceBraveTouchProcessor(_battleMgr, battleCardBase2, choiceSkills)));
							_battleMgr.BattleUIContainer.DisableMenu();
						}
						else
						{
							StartOpenHandDetail(battleCardBase2, right: false, isChoiceBraveButton: true);
						}
					}
					if (CheckChoiceBraveButton(hits, "EnemyChoiceBraveButton") && flag2)
					{
						BattleCardBase card = _battleMgr.BattleEnemy.Class;
						StartOpenHandDetail(card, right: false, isChoiceBraveButton: true);
					}
				}
			}
			else
			{
				if (!battleCardBase.IsOnDraw)
				{
					_hitCard = battleCardBase;
					if (BattleManagerBase.UseCustomMouse)
					{
						if (_battlePlayerView.IsDetailOn() && array == null)
						{
							array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
						}
						if (!_battlePlayerView.IsDetailOn() || !array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
						{
							Func<BattleCardBase, bool> func = (BattleCardBase battleCardBase3) => _inputMgr.IsDownMoved() && _pressedCard != null && _pressedCard == battleCardBase3;
							if ((_inputMgr.IsClick() || func(_hitCard)) && flag && BattlePlayer.IsSelfTurn && _hitCard.IsInHand && _hitCard.IsPlayer && !_hitCard.IsClass && WillPlayCardFromHand(showAlert: true))
							{
								SelectCardProcessor touchProcessor3 = new SelectCardProcessor(_battleMgr, _hitCard, _inputMgr, _pressedCard != null);
								parallelVfxPlayer.Register(RegisterTouchProcessor(touchProcessor3));
							}
						}
					}
					else if ((_hitCard.IsPlayer || _battleMgr.GameMgr.IsWatchBattle) && _hitCard.IsInHand && (_battleMgr.GameMgr.IsAdmin || _hitCard.IsPlayer))
					{
						SelectCardProcessor touchProcessor4 = new SelectCardProcessor(_battleMgr, _hitCard, _inputMgr, _pressedCard != null);
						parallelVfxPlayer.Register(RegisterTouchProcessor(touchProcessor4));
						if (battleCardBase.BattleCardView.Transform.localScale.x >= 1f)
						{
							StartOpenHandDetail(_hitCard, right: false);
						}
					}
					if (_hitCard.IsInplay && !_hitCard.IsClass)
					{
						bool flag4 = true;
						if (BattleManagerBase.UseCustomMouse)
						{
							if (_battlePlayerView.IsDetailOn() && array == null)
							{
								array = Physics.RaycastAll(ray2.origin, ray2.direction, float.PositiveInfinity);
							}
							if (_battlePlayerView.IsDetailOn() && array.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
							{
								flag4 = false;
							}
							else
							{
								if (_hitCard.IsPlayer && !CanInPlayCardBeDragged(_hitCard, _battleMgr.BattlePlayer))
								{
									_alertCard = _hitCard;
								}
								if (_inputMgr.IsDownMoved())
								{
									flag4 = _pressedCard == _hitCard;
								}
								else if (!_inputMgr.IsClick())
								{
									flag4 = false;
								}
								if (flag4 && (_detailCardDisplay == _hitCard || _detailCardStart == _hitCard))
								{
									HideDetailPanel();
								}
							}
						}
						if (flag4)
						{
							EvolutionSimpleProcessor evolutionProcessor = CreateEvolutionSimpleProcessor(_hitCard);
							_detailPanelTouchProcessor = new DetailPanelTouchProcessor(_battleMgr, _hitCard, _inputMgr, _prediction, evolutionProcessor);
							return RegisterTouchProcessor(_detailPanelTouchProcessor);
						}
					}
				}
				if (flag2 && battleCardBase.IsClass)
				{
					if (battleCardBase.IsPlayer && !_battlePlayerView.HandControl.IsHandStateFocus())
					{
						_isTapPlayerClass = true;
						ClassBuffTouchProcessor touchProcessor5 = CreateClassBuffTouchProcessor(_battleMgr, battleCardBase, _inputMgr);
						return RegisterTouchProcessor(touchProcessor5);
					}
					if (!battleCardBase.IsPlayer && !BattleEnemy.BattleEnemyView.HandControl.IsHandStateFocus())
					{
						ClassBuffTouchProcessor touchProcessor6 = CreateClassBuffTouchProcessor(_battleMgr, battleCardBase, _inputMgr);
						return RegisterTouchProcessor(touchProcessor6);
					}
				}
				else if (flag2)
				{
					WatchChoiceDetail(battleCardBase, parallelVfxPlayer);
				}
			}
		}
		if (flag2)
		{
			if (_battleCamera._backgroundCamera.fieldOfView != _battleCamera.Camera.fieldOfView)
			{
				ray = _battleCamera._backgroundCamera.ScreenPointToRay(mousePosition);
				Physics.Raycast(ray.origin, ray.direction, out hitInfo, float.PositiveInfinity);
				hits = Physics.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity);
			}
			bool flag5 = true;
			if (_battleMgr.IsPuzzleMgr)
			{
				_ = new RaycastHit[0];
				Ray ray3 = UIManager.GetInstance().getCamera().ScreenPointToRay(mousePosition);
				flag5 = !Physics.RaycastAll(ray3.origin, ray3.direction, float.PositiveInfinity).Any((RaycastHit obj) => obj.collider.gameObject.name == "Btn");
			}
			if (flag5)
			{
				CheckFieldGimic(hitInfo.collider);
			}
			PlayTapEffect(hits, mousePosition);
		}
		return parallelVfxPlayer;
	}

	protected virtual void WatchChoiceDetail(BattleCardBase hitCard, ParallelVfxPlayer parallelVfx)
	{
	}

	private bool TryZoomHand(RaycastHit[] hits, RaycastHit[] hitsPanel)
	{
		if (_battlePlayerView.IsDetailOn() && hitsPanel != null && hitsPanel.Any((RaycastHit entry) => entry.collider.CompareTag("DetailPanel")))
		{
			return false;
		}
		BattleCardBase battleCardBase = TouchCard(hits);
		if (battleCardBase != null && battleCardBase.IsInHand)
		{
			if (!IsAbleZoomHandInSelecting(battleCardBase))
			{
				return false;
			}
			if (battleCardBase.IsPlayer)
			{
				if (BattlePlayer.HandCardList.Count > 0)
				{
					_battleMgr.VfxMgr.RegisterImmediateVfx(_battlePlayerView.HandFocus());
					_battleMgr.VfxMgr.RegisterImmediateVfx(_battleEnemyView.HandUnfocus());

					return true;
				}
			}
			else if (_battleMgr.GameMgr.IsAdminWatch && !battleCardBase.IsPlayer && BattleEnemy.HandCardList.Count > 0)
			{
				_battleMgr.VfxMgr.RegisterImmediateVfx(_battlePlayerView.HandUnfocus());
				_battleMgr.VfxMgr.RegisterImmediateVfx(_battleEnemyView.HandFocus());

				return true;
			}
		}
		return false;
	}

	private bool IsAbleZoomHandInSelecting(BattleCardBase touchedCard)
	{
		IBattlePlayerView battlePlayerView = ((!touchedCard.IsPlayer) ? _battleEnemyView : _battlePlayerView);
		if (battlePlayerView.IsSelecting)
		{
			if (battlePlayerView.SelectSkillActCard == touchedCard)
			{
				return false;
			}
			if (battlePlayerView.GetSelectCardList().Contains(touchedCard))
			{
				return false;
			}
		}
		return true;
	}

	public bool WillPlayCardFromHand(bool showAlert)
	{
		if (_hitCard == null)
		{
			return false;
		}
		bool flag = _battlePlayerView.DetailOpenCard != null;
		HideDetailPanel();
		if (false || (_hitCard.Movable() && _hitCard.AreCanPlayConditionsFulfilled && IsFeasiblePlayCard()))
		{
			EmitHandUtility.SendSelectObject(_battleMgr, _hitCard);
			_battlePlayerView.MoveCardStart(_hitCard, isEffectAndSoundOn: true);
			_hitCard.BattleCardView.HideHandCardInfo();
			Skill_transform accelerateOrCrystallizeTransformSkill = _hitCard.GetAccelerateOrCrystallizeTransformSkill();
			if (accelerateOrCrystallizeTransformSkill != null)
			{
				BattleCardBase card = _battleMgr.CreateTransformCardRegisterVfx(accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard, accelerateOrCrystallizeTransformSkill.TransformId, accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard.IsPlayer);
				_prediction.Play(card);
			}
			else
			{
				_prediction.Play(_hitCard);
			}
			_inputMgr.WentOverDrag = false;
			return true;
		}
		if (showAlert)
		{
			if (_battlePlayerView.ShowAlertMessageTouchCard(ref _hitCard, ref _battleMgr))
			{
				_alertCard = _hitCard;
			}
			if (flag)
			{
				EmitHandUtility.SendSelectObject(_battleMgr, null);
			}
		}
		return false;
	}

	public BattleCardBase TouchCard(RaycastHit[] hits)
	{
		BattleCardBase result = null;
		if (!_battleMgr.GameMgr.IsWatchBattle && (!_battlePlayerView.IsTouchable() || _battlePlayerView.IsSelecting))
		{
			return result;
		}
		float num = float.PositiveInfinity;
		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit hit = hits[i];
			BattleCardBase hitCardFromRayCastHit = GetHitCardFromRayCastHit(hit);
			if (hitCardFromRayCastHit != null && hit.distance < num)
			{
				result = hitCardFromRayCastHit;
				num = hit.distance;
			}
		}
		return result;
	}

	protected virtual BattleCardBase GetHitCardFromRayCastHit(RaycastHit hit)
	{
		GameObject gameObject = hit.collider.gameObject.transform.parent.gameObject;
		BattleCardBase battleCardBase = BattlePlayer.FindCardFromGameObject(gameObject);
		if (battleCardBase != null)
		{
			return battleCardBase;
		}
		BattleCardBase battleCardBase2 = BattleEnemy.FindCardFromGameObject(gameObject);
		if (battleCardBase2 != null)
		{
			return battleCardBase2;
		}
		return null;
	}

	private bool PlayTapEffect(RaycastHit[] hits, Vector3 currentMousePosition)
	{
		bool flag = false;
		bool flag2 = false;
		Vector3 pos = Vector3.zero;
		Vector3 pos2 = Vector3.zero;
		if (CastRayWithBattleUI(currentMousePosition))
		{
			return false;
		}
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].collider.CompareTag("TapEffectArea1"))
			{
				flag = true;
				pos = hits[i].point;
			}
			else if (hits[i].collider.CompareTag("TapEffectArea2"))
			{
				flag2 = true;
				pos2 = hits[i].point;
			}
			else if (!hits[i].collider.CompareTag("BattleUI"))
			{
				return false;
			}
		}
		if (flag && flag2)
		{
			if (pos.z < pos2.z)
			{
				_backGround.StartFieldTapEffect(1, pos);
			}
			else
			{
				_backGround.StartFieldTapEffect(2, pos2);
			}
		}
		else if (flag)
		{
			_backGround.StartFieldTapEffect(1, pos);
		}
		else if (flag2)
		{
			_backGround.StartFieldTapEffect(2, pos2);
		}
		return flag || flag2;
	}

	public void CheckFieldGimic(Collider touch)
	{
		if (!(touch == null) && (touch.CompareTag("FieldGimic1") || touch.CompareTag("FieldGimic2") || touch.CompareTag("FieldGimic3")))
		{
			_backGround.StartFieldGimic(touch.gameObject);
		}
	}

	public bool CheckEpPanel(Collider touch)
	{
		if (touch.CompareTag("EpPanel"))
		{
			return true;
		}
		return false;
	}

	public bool CheckChoiceBraveButton(RaycastHit[] hits, string tag)
	{
		foreach (RaycastHit raycastHit in hits)
		{
			if (raycastHit.collider.CompareTag(tag))
			{
				return true;
			}
		}
		return false;
	}

	public void StopMovingHandCard(BattleCardBase card)
	{
		_pressedCard = null;
		_battlePlayerView.CancelCardDrag(card);
		_prediction.Clear();
	}

	public void StartOpenHandDetail(BattleCardBase card, bool right, bool isChoiceBraveButton = false)
	{
		_detailCardStart = card;
		bool flag = Mathf.Approximately(card.BattleCardView.Transform.localPosition.x, 0f);
		BattleCoroutine.GetInstance().StartCoroutine(HandCardDetail(card, !flag && InputMgr.ShowDetailLeftAndRight && right, isChoiceBraveButton));
	}

	private IEnumerator HandCardDetail(BattleCardBase card, bool showRight, bool isChoiceBraveButton)
	{
		bool wasCardInHandBeforeDetailOpen = card.IsInHand;
		bool wasDetailPanelHidden = false;
		if (_battlePlayerView.IsDetailOn())
		{
			_battlePlayerView.HideDetailPanel();
			wasDetailPanelHidden = true;
		}
		_battleMgr.DetailMgr.DetailPanelControl.SetScreenPosition(showRight);
		yield return new WaitForSeconds(0.1f);
		if (wasCardInHandBeforeDetailOpen && !card.IsInHand)
		{
			_detailCardStart = null;
			yield break;
		}
		if (!card.IsOnMove && _detailCardStart != null)
		{
			DetailPanelControl.ShowRequest showRequest = DetailPanelControl.ShowRequest.NORMAL;
			if (Data.CurrentFormat == Format.Avatar && card.IsClass && card.SelfBattlePlayer.AvatarBattleInfo != null)
			{
				showRequest = (isChoiceBraveButton ? DetailPanelControl.ShowRequest.CHOICE_BRAVE : DetailPanelControl.ShowRequest.CHOICE_BRAVE_AND_BUFF);
			}
			_battlePlayerView.ShowDetailPanel(_battleMgr, _battleMgr.OperateMgr, card, showRequest);
			EmitHandUtility.SendSelectObject(_battleMgr, card);
		}
		else if (wasDetailPanelHidden && !card.IsOnMove)
		{
			EmitHandUtility.SendSelectObject(_battleMgr, null);
		}
		_detailCardStart = null;
	}

	public virtual void ResetDetail()
	{
		if (_battlePlayerView.GetDetailCard() != null)
		{
			HideDetailPanel();
			_hitCard = null;
		}
	}

	public VfxBase RegisterEvolutionTouchProcessor(bool isDetailPanelEvolution = false)
	{
		if (!_battleMgr.BattlePlayer.IsSelfTurn)
		{
			LocalLog.AccumulateLastTraceLog("RegisterEvolutionTouchProcessor NotSelfTurn");
			return NullVfx.GetInstance();
		}
		_battlePlayerView._isEvolutionSkillSelect = true;
		_evolutionTouchProcessor = CreateEvolutionTouchProcessor(_inputMgr, isDetailPanelEvolution);
		_evolutionTouchProcessor.OnFocusTarget += delegate(BattleCardBase targetCard)
		{
			if (targetCard != null && targetCard.CanEvolution(isSkill: false, isSelfBattlePlayer: true) && targetCard.IsInplay)
			{
				this.OnEvolveFocus.Call();
				_battleMgr.VfxMgr.RegisterImmediateVfx(ParallelVfxPlayer.Create(InstantVfx.Create(delegate
				{
					_battlePlayerView.ShowDetailPanel(_battleMgr, _battleMgr.OperateMgr, targetCard, DetailPanelControl.ShowRequest.EVOLUTION_SELECT);
				}), NullVfx.GetInstance(), NullVfx.GetInstance()));
				EmitHandUtility.SendSlideObject(_battleMgr, NetworkBattleSender.SLIDE_OBJECT_TYPE.Evolve, targetCard);
			}
			return NullVfx.GetInstance();
		};
		_evolutionTouchProcessor.OnUnfocusTarget += delegate(BattleCardBase targetCard)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			if (targetCard != null)
			{
				this.OnEvolveUnfocus.Call();
				parallelVfxPlayer.Register(NullVfx.GetInstance());
				parallelVfxPlayer.Register(InstantVfx.Create(delegate
				{
					HideDetailPanel();
				}));
			}
			_battleMgr.VfxMgr.RegisterImmediateVfx(ParallelVfxPlayer.Create(parallelVfxPlayer, _evolutionTouchProcessor.ShowEvolutionMessage()));
			return NullVfx.GetInstance();
		};
		_evolutionTouchProcessor.OnSelectTarget += delegate(BattleCardBase targetCard)
		{
			_battleMgr.VfxMgr.RegisterImmediateVfx(ParallelVfxPlayer.Create(InstantVfx.Create(delegate
			{
				if (IsFeasibleEvol())
				{
					_battlePlayerView.DragArrowStop(_battleMgr);
					if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.CONFIRM_EVOLVE))
					{
						_backGround.SetShaderGlobalColorBG.ChangeGlobalShaderColorFadeIn();
					}
				}
			}), NullVfx.GetInstance(), PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.CONFIRM_EVOLVE) ? _battleMgr.DetailMgr.DetailPanelControl.ShowEvolutionButton(targetCard) : NullVfx.GetInstance()));
			return NullVfx.GetInstance();
		};
		EvolutionTouchProcessor evolutionTouchProcessor = _evolutionTouchProcessor;
		evolutionTouchProcessor.OnAfterEvolveDragSelect = (Func<VfxBase>)Delegate.Combine(evolutionTouchProcessor.OnAfterEvolveDragSelect, this.OnAfterEvolveDragSelect);
		_evolutionTouchProcessor.OnNotSelectTarget += delegate
		{
			_battlePlayerView.DragArrowStop(_battleMgr);
			_battleMgr.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			EmitHandUtility.SendSlideObject(_battleMgr, NetworkBattleSender.SLIDE_OBJECT_TYPE.Cancel);
		};
		Exit();
		return RegisterTouchProcessor(_evolutionTouchProcessor);
	}

	private VfxBase RegisterDeckTouchProcessor(IBattlePlayerView battlePlayerView)
	{
		return RegisterTouchProcessor(new DeckTouchProcessor(battlePlayerView, _inputMgr));
	}

	public static bool IsPlayCard(BattlePlayerBase battlePlayer, BattleCardBase hitCard, bool isDebugLog = false)
	{
		int num = hitCard.Cost;
		if (hitCard.CheckConditionFixedUseCost(isPrePlay: true))
		{
			num = hitCard.CalcFixedUseCost(hitCard.SelfBattlePlayer.Pp);
		}
		if (battlePlayer.Pp < num)
		{
			if (isDebugLog)
			{
				LocalLog.AccumulateTraceLog("IsPlayCard PPover Pp" + battlePlayer.Pp + "useCost" + num + "cardId" + hitCard.CardId + "idx" + hitCard.Index);
			}
			return false;
		}
		if (hitCard.IsSpell)
		{
			return true;
		}
		if (battlePlayer.ClassAndInPlayCardList.Count > 5)
		{
			if (isDebugLog)
			{
				LocalLog.AccumulateTraceLog("IsPlayCard OverField ");
			}
			return false;
		}
		if (hitCard.SelfBattlePlayer.Class.SkillApplyInformation.IsCantPlay(hitCard))
		{
			if (isDebugLog)
			{
				LocalLog.AccumulateTraceLog("IsPlayCard CantPlay ");
			}
			return false;
		}
		return true;
	}

	public VfxBase ForceEndTouchProcessor()
	{
		IsForceEnd = true;
		VfxWith<ITouchProcessor> vfxWith = (HasTouchProcessor ? _touchProcessor.End() : new VfxWith<ITouchProcessor>(NullVfx.GetInstance(), null));
		VfxBase result = vfxWith.Vfx;
		if (vfxWith.Value is ChoiceTouchProcessor || vfxWith.Value is SkillTargetSelectTouchProcessor)
		{
			_battlePlayerView.DisableSettingFlag();
		}
		if (vfxWith.Value is ChoiceTouchProcessor)
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(vfxWith.Value.Start());
			sequentialVfxPlayer.Register(vfxWith.Value.End().Vfx);
			result = sequentialVfxPlayer;
		}
		else if (!(_touchProcessor is FusionWaitProcessor) && _touchProcessor is SetCardProcessor)
		{
			result = vfxWith.Value.End().Vfx;
		}
		_touchProcessor = null;
		IsForceEnd = false;
		return result;
	}

	public void Exit()
	{
		_hitCard = null;
	}

	public VfxBase RegisterTouchProcessor(ITouchProcessor touchProcessor)
	{
		_touchProcessor = touchProcessor;
		VfxBase result = touchProcessor.Start();
		IsProcessorStart = true;
		return result;
	}

	protected virtual bool IsFeasibleAttack()
	{
		return true;
	}

	protected virtual bool IsFeasiblePlayCard()
	{
		return true;
	}

	protected virtual bool IsFeasibleEmote()
	{
		return true;
	}

	protected virtual bool IsFeasibleEvol()
	{
		return true;
	}

	protected virtual SkillTargetSelectTouchProcessor CreateSkillTargetSelectTouchProcessor(BattleCardBase actingCard, InputMgr inputMgr, List<SkillBase> skillList, List<BattleCardBase> selectedCardList, bool isEvolve)
	{
		return SkillTargetSelectTouchProcessor.Create(_battleMgr, actingCard, skillList, _prediction, selectedCardList, isEvolve, isChoiceBrave: false);
	}

	protected Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> CreateSkillTargetSelectTouchProcessorFunc(InputMgr inputMgr)
	{
		return (BattleCardBase actingCard, List<BattleCardBase> selectedCardList, List<SkillBase> skillList, bool isEvolve) => CreateSkillTargetSelectTouchProcessor(actingCard, inputMgr, skillList, selectedCardList, isEvolve);
	}

	public SetCardProcessor CreateSetCardProcessor(BattleCardBase card)
	{
		if (!_battleMgr.BattlePlayer.IsSelfTurn)
		{
			LocalLog.AccumulateLastTraceLog("CreateSetCardProcessor NotSelfTurn");
			return null;
		}
		List<SkillBase> selectSkills = (from s in card.GetSelectTypeSkill()
			where s.OnWhenChoiceBrave == 0
			select s).ToList();
		Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc = CreateSkillTargetSelectTouchProcessorFunc(_inputMgr);
		return new SetCardProcessor(_battleMgr, card, selectSkills, _prediction, getSkillTargetSelectTouchProcessorFunc);
	}

	protected virtual EvolutionTouchProcessor CreateEvolutionTouchProcessor(InputMgr inputMgr, bool isDetailEvolution = false)
	{
		Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc = CreateSkillTargetSelectTouchProcessorFunc(_inputMgr);
		return new EvolutionTouchProcessor(_battleMgr, inputMgr, _battleMgr.BattleResourceMgr, isDetailEvolution, getSkillTargetSelectTouchProcessorFunc, _prediction);
	}

	public EvolutionSimpleProcessor CreateEvolutionSimpleProcessor(BattleCardBase card)
	{
		List<SkillBase> selectSkills = card.GetSelectTypeSkill(isEvolve: true, isFusion: false, isRegister: false, isEvolutionSimpleProcessor: true).ToList();
		Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc = CreateSkillTargetSelectTouchProcessorFunc(_inputMgr);
		return new EvolutionSimpleProcessor(_battleMgr, card, _inputMgr, selectSkills, getSkillTargetSelectTouchProcessorFunc, this.OnStartEvolveSkillTargetSelect);
	}

	protected virtual EmotionTouchProcessor CreateEmotionTouchProcessor(BattleManagerBase battleMgr, InputMgr inputMgr, VfxMgr emotionVfxMgr)
	{
		return new EmotionTouchProcessor(BattlePlayer.PlayerEmotion, battleMgr.BattleResourceMgr, battleMgr.TouchControl, inputMgr, emotionVfxMgr);
	}

	protected virtual ClassBuffTouchProcessor CreateClassBuffTouchProcessor(BattleManagerBase battleMgr, BattleCardBase touchClass, InputMgr inputMgr)
	{
		return new ClassBuffTouchProcessor(battleMgr, touchClass, inputMgr);
	}

	public static bool CanInPlayCardBeDragged(BattleCardBase attackCard, BattlePlayerBase battlePlayer)
	{
		if (battlePlayer.IsSelfTurn && attackCard.IsPlayer && attackCard.IsUnit && attackCard.Attackable)
		{
			return attackCard.AreCanAttackConditionsFulfilled;
		}
		return false;
	}

	public static bool CastRayWithBattleUI(Vector3 mousePosition)
	{
		return CastRayWithTags(mousePosition, new List<string> { "BattleUI" });
	}

	public static bool CastRayWithTags(Vector3 mousePosition, IEnumerable<string> tags)
	{
		Ray ray = UIManager.GetInstance().getCamera().ScreenPointToRay(mousePosition);
		return Physics.RaycastAll(ray.origin, ray.direction, float.PositiveInfinity).Any((RaycastHit hit) => tags.Any((string tag) => hit.collider.CompareTag(tag)));
	}

	private void HideDetailPanel()
	{
		_battlePlayerView.HideDetailPanel();
		_detailCardDisplay = null;
		_detailCardStart = null;
	}

	protected virtual void HideAlert()
	{
		_battlePlayerView.HideAlertDialogue();
		_alertCard = null;
	}

	protected virtual bool IsShowingAlert()
	{
		return _battlePlayerView.IsShowingAlert();
	}

	private void checkHoverCard(RaycastHit[] hits, bool isZoomHand)
	{
		// OptionSettingWindow removed (DEAD-COLD engine cleanup Task 13) — Auto is the headless default; guard removed
		BattleCardBase battleCardBase = TouchCard(hits);
		BattleCardBase battleCardBase2 = null;
		if (KeepAlertCard != null && KeepAlertCard == battleCardBase && IsShowingAlert())
		{
			return;
		}
		KeepAlertCard = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (battleCardBase != null)
		{
			flag = battleCardBase.IsClass;
			flag3 = battleCardBase.IsHoverActionCard();
		}
		else if (CheckChoiceBraveButton(hits, "PlayerChoiceBraveButton") && (_battleMgr.GameMgr.IsWatchBattle || !BattlePlayer.CanChoiceBrave))
		{
			flag2 = true;
			battleCardBase = _battleMgr.BattlePlayer.Class;
		}
		else if (CheckChoiceBraveButton(hits, "EnemyChoiceBraveButton"))
		{
			flag2 = true;
			battleCardBase = _battleMgr.BattleEnemy.Class;
		}
		else
		{
			_isUsingEmote = false;
		}
		if (flag || flag2 || flag3)
		{
			if (flag && battleCardBase.IsPlayer && _isUsingEmote)
			{
				return;
			}
			if (_detailCardDisplay == null)
			{
				battleCardBase2 = battleCardBase;
				_hoverTime = 0f;
				_detailCardHover = null;
			}
			else
			{
				bool flag4 = false;
				if (battleCardBase.IsInHand)
				{
					if (_detailCardDisplay.IsInHand)
					{
						Vector2 to = _inputMgr.GetPos() - _lastMousePos;
						if (to.sqrMagnitude > 1f && Mathf.Abs(Vector2.Angle(Vector2.up, to)) > 70f)
						{
							flag4 = true;
						}
					}
					else
					{
						flag4 = true;
					}
				}
				if (flag4)
				{
					battleCardBase2 = battleCardBase;
				}
				else if (_detailCardHover == battleCardBase)
				{
					_hoverTime += Time.deltaTime;
					if (_hoverTime > 0.2f)
					{
						battleCardBase2 = battleCardBase;
					}
				}
				else
				{
					_detailCardHover = battleCardBase;
					_hoverTime = 0f;
				}
			}
		}
		if (battleCardBase2 == null)
		{
			if (_detailCardDisplay == null && _battlePlayerView.IsDetailOn())
			{
				HideDetailPanel();
			}
			if (_alertCard != null && IsShowingAlert())
			{
				HideAlert();
			}
			return;
		}
		bool flag5 = IsShowingAlert() && battleCardBase2 == _alertCard;
		if ((battleCardBase2 != _battlePlayerView.GetDetailCard() || (battleCardBase2.IsClass && flag2 != _isDetailCardChoiceBrave)) && !flag5)
		{
			if (_alertCard != null || IsShowingAlert())
			{
				HideAlert();
			}
			if (!(_battlePlayerView.HandControl.IsHandStateFocus() && flag) || !battleCardBase2.IsPlayer)
			{
				_detailCardDisplay = battleCardBase2;
				_isDetailCardChoiceBrave = flag2;
				bool right = !battleCardBase2.IsClass && !battleCardBase2.IsInHand && Input.mousePosition.x < (float)Screen.width / 2f;
				StartOpenHandDetail(battleCardBase2, right, flag2);
			}
		}
	}

	// OptionSettingWindow removed (DEAD-COLD engine cleanup Task 13) — shortcut methods return false (headless default)
	private bool UsePlayShortcut() => false;

	private bool UseEvolutionShortcut() => false;

	private bool UseDetailShortcut() => false;

	public Prediction GetPrediction()
	{
		return _prediction;
	}

	protected virtual void StopDraggingArrow()
	{
		_battlePlayerView.DragArrowStop(_battleMgr);
	}
}
