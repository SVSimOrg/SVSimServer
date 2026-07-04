using NUnit.Framework;
using UnityEngine;
using Wizard.Battle.View;

namespace SVSim.BattleEngine.Tests
{
    // Regression for the in-play metamorphose NRE diagnosed 2026-06-07 (bid 283192092460).
    //
    // The IsRecovery card-create delegate (NetworkBattleManagerBase.cs:379) passes null for the
    // cardGameObject, which left BattleCardView.GameObject null. Skill_metamorphose.cs:147 in the
    // IsInplay branch then NRE'd on the unguarded
    //   metamorphosedCard.BattleCardView.GameObject.transform.rotation = Quaternion.identity
    // — a purely cosmetic transform reset that has no corresponding state mutation, but tripped over
    // null-GameObject before the surrounding mutations (ReplaceInPlay, SetUpInplay,
    // FlagCardAsDestroyedBySkill, RemoveFromInPlay) could complete.
    //
    // Fix: ViewUiTouchStubs.cs's BattleCardView.GameObject is now lazily non-null (matches the
    // existing Component.gameObject pattern at UnityShim.cs:94). The shim materializes a no-op
    // GameObject on first read; the cosmetic touch resolves to a no-op assignment instead of NRE.
    [TestFixture]
    public class BattleCardViewShimTests
    {
        [Test]
        public void GameObject_is_lazily_non_null_so_unguarded_recovery_touches_no_op()
        {
            var view = new BattleCardView();

            Assert.That(view.GameObject, Is.Not.Null,
                "BattleCardView.GameObject must be lazily non-null in the shim so unguarded " +
                "Unity touches on the IsRecovery card-create path (which passes null cardGameObject) " +
                "resolve to no-ops instead of NRE-ing.");

            Assert.That(view.GameObject.transform, Is.Not.Null,
                "GameObject.transform must follow the shim's lazy non-null Component pattern (UnityShim.cs:94).");

            Assert.DoesNotThrow(() => view.GameObject.transform.rotation = Quaternion.identity,
                "Skill_metamorphose.cs:147's cosmetic transform.rotation reset on the in-play branch must " +
                "not throw in the headless IsRecovery path (live bid 283192092460: A's Petrification " +
                "on B's in-play card).");
        }

        [Test]
        public void GameObject_is_stable_across_reads_so_a_set_followed_by_read_returns_the_same_instance()
        {
            // Lazy materialization caches the GameObject on first read, so subsequent reads return
            // the same instance — required for any code path that reads .GameObject, mutates it,
            // and reads again (e.g. follower position/rotation/scale set in sequence).
            var view = new BattleCardView();
            var first = view.GameObject;
            var second = view.GameObject;
            Assert.That(second, Is.SameAs(first),
                "lazy GameObject must cache; otherwise the second read returns a fresh instance " +
                "and any mutation on the first read is lost.");
        }
    }
}
