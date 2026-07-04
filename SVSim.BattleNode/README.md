# SVSim.BattleNode

Socket.IO node-server emulation for in-battle real-time traffic — the second of prod's 4-server
topology. Handles `Matched` / `BattleStart` / `Deal` / per-action `PlayActions` / `Echo` /
`TurnEnd` between a client and a server-side opponent, for TK2 PvP and AI rank battles.

## Documentation lives in the outer repo

This project's canonical reference is a single hub doc in the **outer** SVSim repo (this directory
is an inner git repo, so the doc isn't tracked alongside the code):

→ **`docs/battle-node.md`** (from the SVSim root) — architecture, the dispatch matrix by battle
type, connect handshake + crypto, `BattleFinish` wire-result semantics, SIO/EIO event coverage,
reliability (pubSeq/playSeq/Gungnir), wire-format gotchas, where-to-extend, the manual smoke
walkthrough, and the consolidated open-items list.

Relative path from here: [`../../../docs/battle-node.md`](../../../docs/battle-node.md).

Detailed per-URI wire shapes are in `docs/api-spec/in-battle/`; the hub links into them.

Keep `docs/battle-node.md` updated in the same change whenever you alter node behavior.
