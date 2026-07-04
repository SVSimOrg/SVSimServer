// AUTHORED SHIM (not copied). Closure stubs for NESTED types whose unqualified
// references survive in net-new generated no-op shells. The generator emits each
// shell as a base-less `partial class` (it never re-emits the decomp base clause),
// so a derived view that referenced an inherited nested type (e.g. UnitBattleCardView
// using BattleCardView.BuildInfo) loses the inheritance path and the name resolves
// only at NAMESPACE scope. Declaring these top-level in the decomp namespace lets the
// shell's signatures resolve. Safe because nothing crosses the engine boundary with
// these types (zero CS0029 mismatches) — they exist purely so the no-op shells compile.

namespace Wizard.Battle.View
{
}

namespace Wizard.Battle.UI
{
}

namespace Wizard.RoomMatch
{
}

namespace Wizard.RoomMatch
{
}

namespace Wizard.Battle.View.Vfx
{
}
