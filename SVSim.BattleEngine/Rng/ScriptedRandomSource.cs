using System;
using System.Collections.Generic;
using System.Linq;

namespace SVSim.BattleEngine.Rng
{
    // Deterministic source feeding a pre-scripted sequence. Used by oracles to control which outcome a
    // roll selects, and the precursor to the Phase-3 capture-replay source (feed a captured rand list).
    // Throws on overrun so an unexpected extra engine roll fails loudly.
    public sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<double> _units;
        private readonly Queue<int> _selfPicks;

        public ScriptedRandomSource(IEnumerable<double> units, IEnumerable<int> selfPicks = null)
        {
            _units = new Queue<double>(units ?? Enumerable.Empty<double>());
            _selfPicks = new Queue<int>(selfPicks ?? Enumerable.Empty<int>());
        }

        public double NextUnit()
        {
            if (_units.Count == 0)
                throw new InvalidOperationException("ScriptedRandomSource: NextUnit overrun (more synced rolls than scripted)");
            return _units.Dequeue();
        }

        public int NextSelf(int max)
        {
            if (_selfPicks.Count == 0)
                throw new InvalidOperationException("ScriptedRandomSource: NextSelf overrun (more self rolls than scripted)");
            return _selfPicks.Dequeue();
        }
    }
}
