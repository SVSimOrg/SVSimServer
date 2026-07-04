// Per-test isolation: after the Phase-5 ambient rip (chunks 38-47), every piece of per-battle
// mutable state lives on the mgr instance itself; there is no shared ambient to leak across
// fixtures. Residual process-globals (Unity Resources shim cache, Wizard.LocalLog accumulators)
// are already thread-safe (ConcurrentDictionary / static lock), so fixtures run in parallel.
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
