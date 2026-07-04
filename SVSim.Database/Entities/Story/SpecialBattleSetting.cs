using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Entities.Story;

public class SpecialBattleSetting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public int PlayerFirstTurn { get; set; }
    public int PlayerStartPp { get; set; }
    public int EnemyStartPp { get; set; }
    public int PlayerStartLife { get; set; }
    public int EnemyStartLife { get; set; }
    public string PlayerAttachSkill { get; set; } = string.Empty;
    public string EnemyAttachSkill { get; set; } = string.Empty;
    public string IdOverrideInBattleLog { get; set; } = string.Empty;
    public string BanishEffectOverride { get; set; } = string.Empty;
    public string TokenDrawEffectOverride { get; set; } = string.Empty;
    public string SpecialTokenDrawEffectOverride { get; set; } = string.Empty;
    public int ResultSkip { get; set; }
    public int VsEffectOverride { get; set; }
    public int ClassDestroyEffectOverride { get; set; }
    public string? Note { get; set; }
}
