using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.View;

public interface IBattleCardView
{
	Vector3 ForecastIconPosition { get; }

	Vector3 ForecastIconScale { get; }

	float OriginalRootYPosition { get; }

	IReadOnlyBattleCardInfo CardInfo { get; }

	BattlePlayerReadOnlyInfoPair PlayerInfoPair { get; }

	IReadOnlyVoiceInfo VoiceInfo { get; }

	GameObject GameObject { get; }

	GameObject CardWrapObject { get; }

	Transform Transform { get; }

	CardTemplate CardTemplate { get; }

	BoxCollider Collider { get; }

	BattleCardIconAnimations BattleCardIconAnimations { get; }

	Func<bool> GetIsOnMove { get; }

	bool InPlayModelActive { get; set; }

	BattleCamera m_BattleCamera { get; }

	BackGroundBase m_BackGround { get; }

	HandParameter HandParam { get; }

	BattleCardView.AttackTargetSelectInfo _attackTargetSelectInfo { get; set; }

	InPlayCardFrameEffectControl _inPlayFrameEffect { get; set; }

	bool areArrowsForcedOff { get; set; }

	bool _isCardQueuedToBePlayed { get; set; }

	bool isHiddenFromHandView { get; set; }

	bool isHiddenFromInPlayView { get; set; }

	bool isHideFrameEffect { get; set; }

	bool _hasCardEnteredPlayQueue { get; set; }

	bool playVoiceOnDeath { get; set; }

	Coroutine _inPlayRearrangeCoroutine { get; set; }

	Coroutine _waitUntilCardIsInQueueCoroutine { get; set; }

	bool IsNullView { get; }

	bool IsLoadResorces { get; }

	void InitializeVoiceInfo(int cardID);

	void SetupIconAnimations(BattleCardBase card, SkillCollectionBase skills);

	VfxBase LoadResource();

	VfxBase GetResourcePathes(List<BattleManagerBase.ResourceInfo> resourceInfos);

	VfxBase LoadChoiceTransformCardsResources(BattleCardBase card);

	VfxBase GetChoiceTransformCardsResourcePathes(BattleCardBase card, List<BattleManagerBase.ResourceInfo> resourceInfos, bool isRecoveryFinish = false);

	void ResetTemplate();

	bool HasChild(string objectName);

	void AttachChild(string objectName, GameObject gameObject, bool isDestoryEarlierAttached = false);

	void ReserveAttachChild(string objectName);

	bool HasReservedAttachChild(string objectName);

	GameObject DetachChild(string objectName);

	void DestroyChild(string objectName);

	VfxBase UnloadResource();

	void UpdateMovability();

	void HideCanPlayEffect();

	GameObject GetCardMeshGameObject();

	void UpdateParameterView(int offence, int life, int cost, string name, bool isOnField, bool isRecovery = false, bool useNormalCost = false);

	void UpdateOffence(int offence);

	void UpdateLife(int life);

	void UpdateCost(List<int> costList, bool isGenerateInHand = true, bool playEffect = true, bool isForceUpdate = false, bool isOnlyFixedUseCost = false);

	List<int> GetUseCostList(int cost, bool useNomalCost = false);

	void UpdateCostWithoutFixedUse(int cost);

	void SetTillingAndOffset(Vector2 tilling, Vector2 offset);

	void SetVoiceFileCueName(string cueName);

	void PlayVoice(string voiceName);

	void StopVoice();

	void ShowInHandFrameEffect(bool enable);

	void ShowInHandFrameEffect(bool enable, HandCardFrameEffectType type);

	void ShowFusionMetamorphoseFrameEffect(bool enable);

	VfxBase ResetCardView(CardParameter baseParameter);

	VfxBase RecoveryInPlay();

	VfxBase RecoveryInHand();

	VfxBase ShowHandCardInfo(bool isRecovery = false, bool modifyParameterLabel = true);

	void HideHandCardInfo();

	VfxBase ShowAttackFinished();

	VfxBase ShowAttackFinished(SkillBase skill);

	void HideAttackFinished();

	VfxBase InitializeBattleCardIcon(BattleCardBase card, SkillCollectionBase collection, bool isStackWhiteRitual = false);

	VfxBase InitializeBattleCardStackIcon(BattleCardBase card, SkillCollectionBase collection);

	VfxBase ShowBattleCardIcon();

	void SetCostLabelEnable(bool isEnable);

	void SetNormalLabelEnable(bool isEnable);

	GameObject GetChild(string objectName);

	void InitHandParameter();

	void UpdateCostViewStrategy(bool isForceUpdate = false);

	void InitHandParameterIconPos(HandParameter.IconLayout layout);

	VfxBase UpdateBattleCardIconLabelNumber(BattleCardBase card, SkillCollectionBase collection);

	VfxBase UpdateStackWhiteRitualIconNumber();

	void SetCutInLayerNormalObject();

	void ResetPlayQueueFlags();

	void SetParameterIconEnable(bool isEnable);

	VfxBase AddBattleCardIcon(string iconType, string iconFileName);

	VfxBase DeleteBattleCardIcon(string iconType);

	void SetNotCancelColliderEnable(bool isEnable);

	void InitCostViewAnim();

	VfxBase LoadEvolveFrameEffect();

	VfxBase HideBattleCardIcon();
}
