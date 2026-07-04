// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\IReadOnlyVoiceInfo.cs
namespace Wizard.Battle.View
{
public partial interface IReadOnlyVoiceInfo
{
        bool HasSummonTokenVoice { get; set; }
        string VoiceId { get; set; }
        VoiceAndWaitTime GetPlayVoice(IReadOnlyBattleCardInfo cardInfo, BattlePlayerReadOnlyInfoPair playerPair, int executedFixedUseCostIndex, int skillVoiceIndex);
        VoiceAndWaitTime GetSummonTokenVoice();
        VoiceAndWaitTime GetEvolutionVoice();
        VoiceAndWaitTime GetAttackVoice(bool isEvolution);
        VoiceAndWaitTime GetDestroyVoice(bool isEvolution, bool isExecutedWhiteRitual);
        VoiceAndWaitTime GetSkillVoice(bool isEvolution, int skillIndex);
        int GetSkillVoiceCount(bool isEvolution);
        void SetDestroyCardId(int id);
        int AddAttachSkillVoice(string id);
        string GetAttachSkillVoice(int index);
}
}
