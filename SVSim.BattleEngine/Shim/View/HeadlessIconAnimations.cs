// AUTHORED SHIM (not copied). Factory for the no-op BattleCardIconAnimations the headless
// IBattleCardView.BattleCardIconAnimations getter hands back (see _IfaceImpl.g.cs).
//
// BattleCardIconAnimations.Initialize (which seats its private `collection`) is a deferred VFX that
// never runs headless, so a bare `new BattleCardIconAnimations()` leaves `collection` null. The receive
// play path calls BattlePlayerBase.UpdateInPlayBattleCardIconLabel -> HasInductionNumberSkill, which
// iterates `collection.Count()` and NREs on the null. Seed `collection` with an EMPTY
// SkillCollectionBase so the induction-label check resolves to a clean `false` (no cosmetic
// induction-number VFX to play headless). The collection is intentionally empty rather than the played
// card's real skills: the only consumer on the resolve path is this cosmetic icon-label gate, which a
// no-op (empty) collection satisfies correctly. Nothing here touches authoritative game state.

using System.Reflection;

namespace Wizard.Battle.View
{
    internal static class HeadlessIconAnimations
    {
        private static readonly FieldInfo CollectionField =
            typeof(global::BattleCardIconAnimations).GetField("collection",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new System.InvalidOperationException("BattleCardIconAnimations.collection field not found");

        public static global::BattleCardIconAnimations Create()
        {
            var anims = new global::BattleCardIconAnimations();
            CollectionField.SetValue(anims, new global::SkillCollectionBase(null));
            return anims;
        }
    }
}
