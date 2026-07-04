// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\BattleCardView.cs
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
public partial class AttackTargetSelectInfo
{
        public readonly Queue<AttackSelectControl.AttackPair> _attackPairsCardIsInvolvedIn;
        public AttackSelectControl.AttackPair CurrentAttackPairCardIsInvolvedIn { get; set; }
        public bool IsCardInvolvedInAttack { get; set; }
        public bool IsUneffectedByAttackTargetting { get; set; }
        public AttackTargetSelectInfo() { }
        public virtual Action DisableAttackTargettingMovement() => default!;
}
}
}
