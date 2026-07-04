namespace SVSim.Database.Models.Config;

/// <summary>
/// When <see cref="Enabled"/>, fresh-signup viewers (via <c>/tool/signup</c> ->
/// <see cref="SVSim.Database.Repositories.Viewer.ViewerRepository.RegisterAnonymousViewer"/>)
/// are initialised at <c>MissionData.TutorialState = 100</c> — the post-tutorial baseline —
/// instead of the prod default of <c>1</c> (TUTORIAL_STEP0). Intended for local dev / two-
/// client PVP smoke where walking through the tutorial after every wiped identity is dead
/// time. Off by default so prod-replicated captures still exercise the real tutorial flow.
/// <para>
/// This only affects the anonymous signup path. <see cref="SVSim.Database.Repositories.Viewer.ViewerRepository.RegisterViewer"/>
/// (admin import + Steam-social) already lands at state 100 unconditionally.
/// </para>
/// </summary>
[ConfigSection("SkipTutorial")]
public class SkipTutorialConfig
{
    public bool Enabled { get; set; } = false;

    public static SkipTutorialConfig ShippedDefaults() => new();
}
