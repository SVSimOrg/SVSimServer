using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Entities.Story;

public class StoryChapter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int StoryId { get; set; }

    public int SectionId { get; set; }
    public StorySection? Section { get; set; }

    public int CharaId { get; set; }
    public string ChapterId { get; set; } = string.Empty;
    public string NextChapterId { get; set; } = string.Empty;
    public string? RequiredChapterId { get; set; }

    public string? SelectionDisplayPosition { get; set; }
    public string? SelectionTextId { get; set; }
    public decimal XCoordinate { get; set; }
    public decimal YCoordinate { get; set; }
    public int ShowCoordinate { get; set; }
    public int IsCameraMovable { get; set; }
    public int ShowSubtitles { get; set; }

    public bool BattleExists { get; set; }
    public int EnemyCharaId { get; set; }
    public int EnemyClass { get; set; }
    public int EnemyAiId { get; set; }
    public string BgFileName { get; set; } = string.Empty;
    public string? ChapterEffectPath { get; set; }
    public string? ChapterClearTextId { get; set; }
    public int Battle3dFieldId { get; set; }
    public string BgmId { get; set; } = string.Empty;

    public int? SpecialBattleSettingId { get; set; }
    public SpecialBattleSetting? SpecialBattleSetting { get; set; }

    public int ReleasePoint { get; set; }
    public string? UnlockText { get; set; }
    public bool IsMaintenanceChapter { get; set; }
    public bool IsPlayAnotherEndAppearanceAnimation { get; set; }
    public bool IsReleasedAnotherEnd { get; set; }
    public bool IsSkipEnabled { get; set; }

    // Owned collections — populated via .OwnsMany() in DbContext.
    public List<StoryChapterBattleSetting> BattleSettings { get; set; } = new();
    public List<StoryChapterReward> Rewards { get; set; } = new();
    public List<StorySubChapter> SubChapters { get; set; } = new();
}
