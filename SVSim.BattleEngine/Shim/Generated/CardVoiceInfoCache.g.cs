// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\CardVoiceInfoCache.cs
// TODO(engine-cleanup-pass2): 4 of 5 methods unrun in baseline
//   Type: Wizard.Battle.View.CardVoiceInfoCache
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

using System.Collections.Generic;
namespace Wizard.Battle.View
{
public partial class CardVoiceInfoCache
{
        public static void ClearCardVoiceInfo() { }
        public static IReadOnlyVoiceInfo GetCardVoiceInfoForBattle(int cardID) => HeadlessVoiceInfo.Instance; // HEADLESS-FIX (M7): non-null voice info for the IsRecovery death-voice tail
}
}
