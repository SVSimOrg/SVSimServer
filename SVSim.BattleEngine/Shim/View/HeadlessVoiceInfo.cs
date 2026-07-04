// TODO(engine-cleanup-pass2): 12 of 14 methods unrun in baseline
//   Type: Wizard.Battle.View.HeadlessVoiceInfo
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace Wizard.Battle.View
{
    // HEADLESS-FIX (M7): a non-null IReadOnlyVoiceInfo singleton for the headless death-voice tail.
    //
    // SkillProcessor.SelectCardToHaveDestroyVoicePlay (the cosmetic post-Process step that picks which
    // destroyed card plays its death voice) unconditionally dereferences
    // `card.BattleCardView.VoiceInfo.GetDestroyVoice(...).Voice` AND, when IsRecovery is set,
    // `CardVoiceInfoCache.GetCardVoiceInfoForBattle(id).GetDestroyVoice(...).Voice`. Both seams are
    // M1 `default!` shadows headless (BattleCardView is a null view; the voice cache is never primed),
    // so the left operand of that `||` NREs before board-removal can be asserted. The destroy itself
    // (board removal + cemetery move) already committed in the authoritative part of PlayCard upstream;
    // this is purely the audio tail.
    //
    // The real ReadOnlyVoiceInfo can't be reused here: m1_stub_gen dropped its `: IReadOnlyVoiceInfo`
    // base (interfaces are stripped to avoid CS0535 on the no-op stub) and its Get*Voice still return
    // null. So this hand singleton implements the interface directly, returning the engine's own
    // VoiceAndWaitTime._nullVoice sentinel (Voice == "") from every voice getter — the faithful
    // "no voice configured" result for a headless run with no audio. With Voice == "" both operands of
    // the IsNullOrEmpty check are false, the selector returns null, and no voice plays.
    public sealed class HeadlessVoiceInfo : IReadOnlyVoiceInfo
    {
        public static readonly HeadlessVoiceInfo Instance = new HeadlessVoiceInfo();

        public bool HasSummonTokenVoice { get; set; }
        public string VoiceId { get; set; } = "";

        public VoiceAndWaitTime GetPlayVoice(IReadOnlyBattleCardInfo cardInfo, BattlePlayerReadOnlyInfoPair playerPair, int executedFixedUseCostIndex, int skillVoiceIndex) => VoiceAndWaitTime._nullVoice;
        public VoiceAndWaitTime GetSummonTokenVoice() => VoiceAndWaitTime._nullVoice;
        public VoiceAndWaitTime GetEvolutionVoice() => VoiceAndWaitTime._nullVoice;
        public VoiceAndWaitTime GetAttackVoice(bool isEvolution) => VoiceAndWaitTime._nullVoice;
        public VoiceAndWaitTime GetDestroyVoice(bool isEvolution, bool isExecutedWhiteRitual) => VoiceAndWaitTime._nullVoice;
        public VoiceAndWaitTime GetSkillVoice(bool isEvolution, int skillIndex) => VoiceAndWaitTime._nullVoice;
        public int GetSkillVoiceCount(bool isEvolution) => 0;
        public void SetDestroyCardId(int id) { }
        public int AddAttachSkillVoice(string id) => 0;
        public string GetAttachSkillVoice(int index) => "";
    }
}
