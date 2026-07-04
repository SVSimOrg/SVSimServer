// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\PlayerClassBattleCardView.cs
// CONTAINS HAND-EDITS (HEADLESS-FIX). Before any regen, grep this file for "HEADLESS-FIX" and re-apply
// those blocks; a plain regen will clobber them.
using UnityEngine;
using Wizard.Battle.Player.ClassCharacter;
namespace Wizard.Battle.View
{
public partial class PlayerClassBattleCardView
{
        public IClassCharacter ClassCharacter { get; set; }
        public PlayerClassBattleCardView(BuildInfo buildInfo) : base(buildInfo) { } // HEADLESS-FIX (M-HC-4a): chain BuildInfo so the leader view's CardInfo resolves (attack-on-leader targetting)
}
}
