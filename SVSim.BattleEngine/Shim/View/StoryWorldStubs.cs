// AUTHORED SHIM (not copied). Non-battle Story / StorySelectionWorld / Profile UI
// types that the copied engine references only as field/parameter TYPES (never drives
// headless). Generating them full-surface pulled a large non-battle closure
// (BackgroundData/StoryWorldData/IResourceHandle/animation managers, etc.); empty
// no-op stubs in their decomp namespaces resolve the references without the closure.

namespace Wizard.Story.ChapterSelection.FlowChart { }

namespace Wizary.StorySelectionWorld
{
    public class StorySelectionWorldScene { public static int? RedirectSectionId { get; set; } }
}

namespace Wizard.Scenario2.Resource { }

namespace Wizard.UI.Profile
{
}

namespace Wizard.Story.ChapterSelection
{
    public partial class StoryChapterSelectionUtility { }
}
