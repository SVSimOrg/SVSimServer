// AUTHORED SHIM (not copied). The VFX layer, headless. VfxBase mirrors the real
// public surface (see decomp Wizard.Battle.View.Vfx/VfxBase.cs) so the ~800 engine
// call sites that pass/return VfxBase compile unchanged. Containers preserve the
// CONTROL-FLOW contract (Register collects, IsVfxNonEmpty reflects child count) but
// perform NO rendering. InstantVfx stores its action and NEVER runs it: headless
// resolution runs with IsForecast=true so VFX is never played, and per design §3.3
// all state mutation happens synchronously in the skill methods BEFORE the VFX is
// built -- so a never-played VFX loses no game state.
using System;
using System.Collections.Generic;

namespace Wizard.Battle.View.Vfx
{
    public interface IEffectVfx { }

    public class VfxBase
    {
        public virtual bool IsEnd { get; protected set; } = true;
        public virtual void Update(float dt, List<IEffectVfx> effectVfxList) { }
        public virtual void Play() { }
        public virtual bool IsVfxNonEmpty() => false;
    }

    public sealed class NullVfx : VfxBase
    {
        private static readonly NullVfx _ins = new NullVfx();
        public static NullVfx GetInstance() => _ins;
        public override bool IsVfxNonEmpty() => false;
    }

    public sealed class InstantVfx : VfxBase
    {
        // Stored, never invoked ON THE DIRECT ActionProcessor PATH (headless suppression -- see file
        // header: the mutation already happened synchronously before the VFX is built, so the M2-M12
        // direct-path oracles never need to run it). BUT on the RECEIVE-CONDUCTOR path the conductor
        // FUSES the authoritative mutation INTO this delegate (NetworkOperationCollectionBase.cs:63/73/86
        // register an InstantVfx whose body IS the play mutation). A node-side VfxMgr that executes the
        // registered InstantVfx (see HeadlessConductorVfxMgr) calls Run() to fire that mutation. Run() is
        // opt-in: the shared/default VfxMgr still no-ops registration, so the direct path is untouched.
        private Action _action;
        public static InstantVfx Create(Action action) => new InstantVfx { _action = action };
        public override bool IsVfxNonEmpty() => true;
        public void Run() => _action?.Invoke();
    }

    public sealed class WaitVfx : VfxBase
    {
        public static WaitVfx Create(float seconds) => new WaitVfx();
        public override bool IsVfxNonEmpty() => false;
    }

    // Container players: collect children, report non-empty by count, render nothing.
    public class SequentialVfxPlayer : VfxBase
    {
        protected readonly List<VfxBase> _children = new List<VfxBase>();
        public static SequentialVfxPlayer Create() => new SequentialVfxPlayer();
        public static SequentialVfxPlayer Create(IEnumerable<VfxBase> vfxCollection)
        { var p = new SequentialVfxPlayer(); if (vfxCollection != null) p._children.AddRange(vfxCollection); return p; }
        public static SequentialVfxPlayer Create(params VfxBase[] vfxCollection)
        { var p = new SequentialVfxPlayer(); if (vfxCollection != null) p._children.AddRange(vfxCollection); return p; }
        public void Register(VfxBase vfx) { if (vfx != null) _children.Add(vfx); }
        public override bool IsVfxNonEmpty()
        {
            foreach (var c in _children) { if (c != null && c.IsVfxNonEmpty()) return true; }
            return false;
        }
    }

    public class ParallelVfxPlayer : SequentialVfxPlayer
    {
        public static new ParallelVfxPlayer Create() => new ParallelVfxPlayer();
        public static ParallelVfxPlayer Create(IEnumerable<VfxBase> vfxCollection)
        { var p = new ParallelVfxPlayer(); if (vfxCollection != null) p._children.AddRange(vfxCollection); return p; }
        public static ParallelVfxPlayer Create(params VfxBase[] vfxCollection)
        { var p = new ParallelVfxPlayer(); if (vfxCollection != null) p._children.AddRange(vfxCollection); return p; }
        public List<VfxBase> GetVfxList() => new List<VfxBase>(_children);
    }

    public class VfxWithLoading : SequentialVfxPlayer
    {
        public static new VfxWithLoading Create() => new VfxWithLoading();
        public static VfxWithLoading Create(VfxBase mainVfx) => new VfxWithLoading();
        public static VfxWithLoading Create(VfxBase loadingVfx, VfxBase mainVfx) => new VfxWithLoading();
        public virtual VfxBase LoadingVfx => NullVfx.GetInstance();
        public virtual VfxBase MainVfx => NullVfx.GetInstance();
    }

    public class VfxWithLoadingSequential : VfxWithLoading
    {
        public static new VfxWithLoadingSequential Create() => new VfxWithLoadingSequential();
        public static VfxWithLoadingSequential Create(params VfxBase[] mainVfxCollection) => new VfxWithLoadingSequential();
        public override VfxBase LoadingVfx => NullVfx.GetInstance();
        public override VfxBase MainVfx => NullVfx.GetInstance();
        public void RegisterToLoadingVfx(VfxBase vfxToRegister) { }
        public void RegisterToMainVfx(VfxBase vfxToRegister) { }
        public void RegisterVfxWithLoading(VfxWithLoading vfxWithLoadingToRegister) { }
    }

    // Non-generic base (engine references bare `VfxWith` as well as the generics).
    public class VfxWith : VfxBase
    {
        public VfxBase Vfx { get; set; }
    }

    // One-value pair (engine reads .Value / .Vfx).
    public class VfxWith<T> : VfxWith
    {
        public T Value { get; set; }
        public VfxWith() { }
        public VfxWith(VfxBase vfx, T value) { Vfx = vfx; Value = value; }
    }

    // Two-value pair (engine reads .Value_1 / .Value_2 / .Vfx).
    public class VfxWith<T1, T2> : VfxWith
    {
        public T1 Value_1 { get; set; }
        public T2 Value_2 { get; set; }
        public VfxWith() { }
        public VfxWith(VfxBase vfx, T1 value1, T2 value2) { Vfx = vfx; Value_1 = value1; Value_2 = value2; }
    }

    public class EvolveVfxBase : VfxBase { }


    // The VFX manager: headless, registration is suppressed (real VfxMgr early-returns
    // when IsForecast; we no-op unconditionally since we never pump the render loop).
    public class VfxMgr
    {
        public virtual bool IsEnd => true;
        public void RegisterImmediateVfx<T>(T vfx) where T : VfxBase { }
        public virtual void RegisterSequentialVfx<T>(T vfx) where T : VfxBase { }
        public List<VfxBase> GetVfxList<TType>() => new List<VfxBase>();
        public virtual void Update(float dt) { }
        public virtual void Cancel() { }
        public void Dispose() { }
    }
}
