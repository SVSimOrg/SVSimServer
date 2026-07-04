using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.Touch;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class AttackSelectControl
{
	public class AttackPair
	{
		public class AttackPairCard
		{
			public IBattleCardView _battleCardView;

			public bool _isReady;

			public bool _hasStartedMoving;

			public AttackPairCard(IBattleCardView battleCardBase)
			{
				_battleCardView = battleCardBase;
			}

			public AttackPairCard(AttackPairCard attackPairCard)
			{
				_battleCardView = attackPairCard._battleCardView;
				_isReady = attackPairCard._isReady;
				_hasStartedMoving = attackPairCard._hasStartedMoving;
			}
		}

		public AttackPairCard _attackInitiator;

		public AttackPairCard _attackTarget;

		public bool IsAttackPairReady
		{
			get
			{
				if (_attackInitiator._isReady)
				{
					return _attackTarget._isReady;
				}
				return false;
			}
		}

		public AttackPair(IBattleCardView attackInitiator, IBattleCardView attackTarget)
		{
			_attackInitiator = new AttackPairCard(attackInitiator);
			_attackTarget = new AttackPairCard(attackTarget);
		}

		public AttackPair(AttackPair attackPair)
		{
			_attackInitiator = new AttackPairCard(attackPair._attackInitiator);
			_attackTarget = new AttackPairCard(attackPair._attackTarget);
		}
	}

	public class WaitUntilAttackPairIsReadyVfx : VfxBase
	{
		private AttackPair _attackPair;

		public WaitUntilAttackPairIsReadyVfx(AttackPair attackPair)
		{
			_attackPair = attackPair;
		}

		public override void Play()
		{
			BattleCoroutine.GetInstance().StartCoroutine(Wait());
		}

		private IEnumerator Wait()
		{
			while (!_attackPair.IsAttackPairReady)
			{
				yield return null;
			}
			IsEnd = true;
		}
	}

	private bool areAttackPairsBeingUpdated;

	private readonly AttackPair currentAttackPair = new AttackPair(null, null);

	private readonly List<AttackPair> successfulAttackPairs = new List<AttackPair>();

	private IBattleCardView currentAttackInitiator
	{
		get
		{
			return currentAttackPair._attackInitiator._battleCardView;
		}
		set
		{
			currentAttackPair._attackInitiator._battleCardView = value;
		}
	}

	private IBattleCardView currentAttackTarget
	{
		get
		{
			return currentAttackPair._attackTarget._battleCardView;
		}
		set
		{
			currentAttackPair._attackTarget._battleCardView = value;
		}
	}

	public virtual void RegisterAttackPair(AttackPair attackPair)
	{
		IBattleCardView battleCardView = attackPair._attackInitiator._battleCardView;
		IBattleCardView battleCardView2 = attackPair._attackTarget._battleCardView;
		if (attackPair == null || battleCardView == null || battleCardView._attackTargetSelectInfo._attackPairsCardIsInvolvedIn == null)
		{
			ResetCardPosition(currentAttackInitiator);
			ResetCardPosition(currentAttackTarget);
			return;
		}
		successfulAttackPairs.Add(attackPair);
		battleCardView._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Enqueue(attackPair);
		battleCardView2._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Enqueue(attackPair);
		if (!areAttackPairsBeingUpdated)
		{
			BattleCoroutine.GetInstance().StartCoroutine(UpdateAttackPairs());
		}
	}

	public void ResetCardOrientationAndStopMovement(IBattleCardView targetCard)
	{
		if (!targetCard._attackTargetSelectInfo.IsUneffectedByAttackTargetting)
		{
			iTween.Stop(targetCard.CardWrapObject);
			targetCard.CardWrapObject.transform.rotation = Quaternion.identity;
		}
	}

	public virtual VfxBase ResetCardAfterAttackOnReplay()
	{
		return InstantVfx.Create(delegate
		{
			for (int i = 0; i < successfulAttackPairs.Count(); i++)
			{
				IBattleCardView battleCardView = successfulAttackPairs[i]._attackInitiator._battleCardView;
				if (battleCardView._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Count > 0)
				{
					battleCardView._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Dequeue();
				}
				ResetCardPosition(battleCardView);
				IBattleCardView battleCardView2 = successfulAttackPairs[i]._attackTarget._battleCardView;
				if (battleCardView2._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Count > 0)
				{
					battleCardView2._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Dequeue();
				}
				ResetCardPosition(battleCardView2);
			}
		});
	}

	public virtual VfxBase ResetCardAfterAttack(IBattleCardView cardToReset)
	{
		return InstantVfx.Create(delegate
		{
			if (cardToReset._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Count > 0)
			{
				cardToReset._attackTargetSelectInfo._attackPairsCardIsInvolvedIn.Dequeue();
			}
			if (cardToReset._attackTargetSelectInfo.IsCardInvolvedInAttack)
			{
				cardToReset._attackTargetSelectInfo.CurrentAttackPairCardIsInvolvedIn._attackTarget._isReady = true;
			}
			ResetCardPosition(cardToReset);
		});
	}

	private void ResetCardPosition(IBattleCardView targetCard)
	{
	}

	public virtual void StartCardIdling(IBattleCardView battleCardView)
	{
		iTween.Stop(battleCardView.CardWrapObject);
		iTween.MoveAdd(battleCardView.CardWrapObject, iTween.Hash("z", 0.025390625f, "time", Random.Range(0.5f, 0.6f), "looptype", iTween.LoopType.pingPong, "easetype", iTween.EaseType.easeInOutQuad));
	}

	private IEnumerator UpdateAttackPairs()
	{
		areAttackPairsBeingUpdated = true;
		while (successfulAttackPairs.Count > 0)
		{
			float t = MotionUtils.CalculateFrameRateIndependantDampingConstant(0.01f, 10f);
			for (int i = 0; i < successfulAttackPairs.Count; i++)
			{
				AttackPair attackPair = successfulAttackPairs[i];
				if (!attackPair.IsAttackPairReady)
				{
					AttackPair.AttackPairCard attackInitiator = attackPair._attackInitiator;
					AttackPair.AttackPairCard attackTarget = attackPair._attackTarget;
					if (attackInitiator._battleCardView._attackTargetSelectInfo.CurrentAttackPairCardIsInvolvedIn == attackPair)
					{
						MoveCardUpwards(attackInitiator, t);
					}
					if (attackTarget._battleCardView._attackTargetSelectInfo.CurrentAttackPairCardIsInvolvedIn == attackPair)
					{
						MoveCardUpwards(attackTarget, t);
					}
				}
			}
			yield return null;
		}
		areAttackPairsBeingUpdated = false;
	}

	private void MoveCardUpwards(AttackPair.AttackPairCard attackPairCard, float t)
	{
		if (false /* Pre-Phase-5b: IsRecovery guard headless-safe as false */)
		{
			attackPairCard._isReady = true;
		}
		else
		{
			if (attackPairCard == null || attackPairCard._battleCardView == null)
			{
				return;
			}
			IBattleCardView battleCardView = attackPairCard._battleCardView;
			if (IsCardTranslatable(battleCardView) && !battleCardView._attackTargetSelectInfo.IsUneffectedByAttackTargetting && !attackPairCard._isReady)
			{
				if (!attackPairCard._hasStartedMoving)
				{
					attackPairCard._hasStartedMoving = true;
					ResetCardOrientationAndStopMovement(battleCardView);
				}
				Transform transform = battleCardView.CardWrapObject.transform;
				if (!IsCardFullyTranslated(battleCardView))
				{
					Vector3 b = CalculateFinalFloatingPosition(battleCardView);
					transform.localPosition = Vector3.Lerp(transform.transform.localPosition, b, t);
				}
				else
				{
					transform.localPosition = CalculateFinalFloatingPosition(battleCardView);
					attackPairCard._isReady = true;
					StartCardIdling(battleCardView);
				}
			}
		}
	}

	private Vector3 CalculateFinalFloatingPosition(IBattleCardView battleCardView)
	{
		Vector3 localPosition = battleCardView.CardWrapObject.transform.transform.localPosition;
		localPosition.z = -100f;
		return localPosition;
	}

	public bool IsCardTranslatable(IBattleCardView cardToTranslate)
	{
		if (cardToTranslate != null)
		{
			return !cardToTranslate.CardInfo.IsClass;
		}
		return false;
	}

	private bool IsCardFullyTranslated(IBattleCardView cardBeingTranslated)
	{
		return Mathf.Abs(cardBeingTranslated.CardWrapObject.transform.localPosition.z - -100f) < 0.1f;
	}

	public static bool CanCardAttackTarget(BattleCardBase Attacker, BattleCardBase Target, IEnumerable<BattleCardBase> TargetInPlayCards)
	{
		bool flag = false;
		bool isClass = Target.IsClass;
		if (TargetInPlayCards.Any((BattleCardBase c) => c.SkillApplyInformation.IsGuard && !c.CantBeFocusedAttack(Attacker)))
		{
			flag = true;
		}
		if (Attacker.SkillApplyInformation.IsIgnoreGuard)
		{
			flag = false;
		}
		if (Attacker.AttackableCount <= 0)
		{
			return false;
		}
		if ((!Attacker.SkillApplyInformation.IsQuick || !Attacker.SkillApplyInformation.IsRush) && !Attacker.Attackable)
		{
			return false;
		}
		if (isClass)
		{
			if (!Attacker.SkillApplyInformation.IsQuick)
			{
				if (Attacker.IsFirstTurn)
				{
					return false;
				}
				if (!Attacker.Attackable)
				{
					return false;
				}
			}
			if (Attacker.IsCantAttackClass)
			{
				return false;
			}
			if (Attacker.SkillApplyInformation.IsForceAttackUnit && Attacker.OpponentBattlePlayer.InPlayCards.Any((BattleCardBase c) => !c.CantBeFocusedAttack(Attacker) && c.IsUnit && !AttackTargetSelectTouchProcessor.CheckAttackToUnitNotHasGuardError(Attacker, c)))
			{
				return false;
			}
		}
		if (!Target.IsInplay)
		{
			return false;
		}
		if (Target.IsField || Target.CantBeFocusedAttack(Attacker))
		{
			return false;
		}
		if (flag && (isClass || !Target.SkillApplyInformation.IsGuard))
		{
			return false;
		}
		if (isClass && Attacker.IsCantAttackClass)
		{
			return false;
		}
		if (Target.IsUnit && Attacker.SkillApplyInformation.IsSkillCantAtkUnit)
		{
			return false;
		}
		if (Target.IsUnit && Attacker.SkillApplyInformation.IsSkillCantAtkUnitBaseCardId && Attacker.SkillApplyInformation.CantAtkUnitBaseCardIdList.Contains(Target.BaseParameter.BaseCardId))
		{
			return false;
		}
		if (!isClass && Attacker.SkillApplyInformation.IsSkillCantAtkUnitNotHasGuard && !Target.SkillApplyInformation.IsGuard)
		{
			return false;
		}
		return true;
	}

	public static bool IsAttackPossible(AIVirtualCard attacker, AIVirtualCard target, BattlePlayerBase opponent)
	{
		if (attacker.BaseCard.Attackable)
		{
			return CanCardAttackTarget(attacker.BaseCard, target.BaseCard, opponent.InPlayCards);
		}
		return false;
	}
}
