// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\BattleCardView.cs
// CONTAINS HAND-EDITS (HEADLESS-FIX). Before any regen, grep this file for "HEADLESS-FIX" and re-apply
// those blocks; a plain regen will clobber them.
using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;
namespace Wizard.Battle.View
{
public partial class BattleCardView
{
public partial class BuildInfo
{
        public IReadOnlyBattleCardInfo cardInfo;
        public BattlePlayerReadOnlyInfoPair _playerInfoPair;
        // HEADLESS-FIX (M-HC-4a): store cardInfo so the headless BattleCardView can expose CardInfo (the
        // backing card) — the receive ATTACK path reads BattleCardView.CardInfo.IsClass via
        // AttackSelectControl.IsCardTranslatable. (Generated stub body was empty; re-apply on regen.)
        public BuildInfo(IReadOnlyBattleCardInfo cardInfo, BattlePlayerReadOnlyInfoPair playerInfoPair, GameObject gameObject, BattleCamera battleCamera, BackGroundBase backGround, IBattleResourceMgr resourceMgr, Func<bool> getIsTouchable, Func<bool> getIsMovable, Func<bool> getIsOnMove, Func<int, bool> getIsFixedUseEnable, Func<bool> getIsActionCard, Func<bool> getIsAbleToAttack, Func<bool> getIsUnableToAttackClass, Func<HandCardFrameEffectType> getHandCardFrameEffectType)
        {
            this.cardInfo = cardInfo;
            // Seeded ahead for the evolve path (M-HC-4b reads GetBattlePlayerInfoPair); not yet read in M-HC-4a.
            this._playerInfoPair = playerInfoPair;
        }
}
}
}
