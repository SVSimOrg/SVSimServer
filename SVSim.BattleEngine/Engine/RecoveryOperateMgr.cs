using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class RecoveryOperateMgr : OperateMgr
{
	public RecoveryOperateMgr(BattleManagerBase battleMgr, TouchControl touchControl)
		: base(battleMgr, touchControl)
	{
	}

	public override VfxBase InitSetCard(BattleCardBase card, bool isPlayer, bool isSelect = false, bool isRecovery = false, bool isChoiceSelect = false, bool isInstant = false, bool registerDirectlyToVfxManager = true, bool isFusionWait = false, bool isChoiceBrave = false)
	{
		return NullVfx.GetInstance();
	}

	public override VfxBase PlayCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards, bool isRecovery = false, List<int> selectChoiceId = null, bool isChoiceBrave = false)
	{
		base.PlayCard(card, isPlayer, selectCards, isRecovery, selectChoiceId, isChoiceBrave);
		return NullVfx.GetInstance();
	}

	public override VfxBase Attack(BattleCardBase attackCard, BattleCardBase targetCard, bool isPlayer)
	{
		base.Attack(attackCard, targetCard, isPlayer);
		return NullVfx.GetInstance();
	}

	public override VfxBase EvolutionCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards, List<int> selectChoiceId = null)
	{
		base.EvolutionCard(card, isPlayer, selectCards, selectChoiceId);
		return NullVfx.GetInstance();
	}

	public override VfxBase BattleCardSelect(BattleCardBase actCard, BattleCardBase Target, bool isPlayer, bool registerEffectsDirectlyToVfxMgr = true, bool isTransformSkill = false, bool isBurialRiteSkill = false, bool isComplete = true)
	{
		SelectCard(actCard, Target, isPlayer, registerEffectsDirectlyToVfxMgr);
		return NullVfx.GetInstance();
	}

	public override VfxBase PlayerTurnEnd(bool isAuto = false)
	{
		TurnEndOperation(isPlayer: true);
		return NullVfx.GetInstance();
	}

	public override VfxBase TurnEndOperation(bool isPlayer)
	{
		base.TurnEndOperation(isPlayer);
		CallOnTurnEndFinish();
		return NullVfx.GetInstance();
	}

	public override VfxBase FusionCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards)
	{
		base.FusionCard(card, isPlayer, selectCards);
		return NullVfx.GetInstance();
	}
}
