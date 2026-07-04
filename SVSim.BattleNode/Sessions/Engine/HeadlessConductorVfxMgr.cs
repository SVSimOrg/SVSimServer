extern alias engine;
using engine::Wizard.Battle.View.Vfx;

namespace SVSim.BattleNode.Sessions.Engine;

/// <summary>The node's receive-conductor VfxMgr (design Headless-Conductor, Candidate B). The engine's
/// receive CONDUCTOR fuses each authoritative mutation INTO an <see cref="InstantVfx"/> delegate and
/// registers it via <c>VfxMgr.RegisterSequentialVfx</c> (NetworkOperationCollectionBase.cs:63/73/86 —
/// the play move; the deal seats its hand synchronously before any VFX). The shared/authored
/// <see cref="VfxMgr"/> NO-OPS registration (correct for the DIRECT ActionProcessor path the M2-M12
/// oracles use, where the mutation already ran synchronously before the VFX was built). On the RECEIVE
/// path the mutation IS the delegate, so the shadow must RUN it.
///
/// <para>This subclass is wired ONLY through <see cref="SessionContentsCreator.CreateVfxMgr"/> (the
/// node's own factory). The HeadlessFixture oracle tests construct their VfxMgr via their own
/// HeadlessContentsCreator (a plain <c>new VfxMgr()</c>), so the M2-M12 direct-path oracles are
/// untouched BY CONSTRUCTION.</para>
///
/// <para>It executes ONLY top-level <see cref="InstantVfx"/> registrations. It deliberately does NOT
/// recurse into container VFX (Sequential/Parallel) — those carry cosmetic nested VFX built over the
/// no-op view leaves, which must stay un-played. The authoritative mutations the receive conductor
/// cares about are always registered as a top-level InstantVfx.</para></summary>
internal sealed class HeadlessConductorVfxMgr : VfxMgr
{
    public override void RegisterSequentialVfx<T>(T vfx)
    {
        if (vfx is InstantVfx instant)
        {
            instant.Run();
        }
        // Non-InstantVfx (containers, waits, cosmetic vfx) are dropped — no render loop headless.
    }
}
