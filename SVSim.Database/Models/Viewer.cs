using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;
using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// A user within the game system.
/// </summary>
[Index(nameof(ShortUdid))]
[Index(nameof(Udid), IsUnique = true)]
public class Viewer : BaseEntity<long>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override long Id { get; set; }

    /// <summary>
    /// This user's name displayed in game.
    /// </summary>
    public string DisplayName { get; set; } = String.Empty;

    /// <summary>
    /// This user's short identifier.
    /// </summary>
    public long ShortUdid { get; set; }

    /// <summary>
    /// The client's full UDID (AES key for the wire protocol). Set when the viewer is created
    /// via <c>/tool/signup</c>; null for viewers created via the admin Steam-import path. Unique
    /// when present — the partial filter is declared in the migration.
    /// </summary>
    public Guid? Udid { get; set; }
    
    public DateTime LastLogin { get; set; }

    /// <summary>
    /// UTC instant of the most recent daily-login-bonus claim. Null = never claimed.
    /// Compared against the daily reset boundary via IGameCalendarService.ResetReady.
    /// </summary>
    public DateTime? LastLoginBonusClaimedAt { get; set; }

    /// <summary>
    /// 1-based position in the Normal login-bonus cycle. 0 = never claimed (first claim
    /// will write 1). Wraps back to 1 after reaching cycle length (LoginBonusConfig.Normal.Count).
    /// </summary>
    public int LoginBonusStreak { get; set; }

    /// <summary>BGType enum: 0=Deck, 1=CustomBG, 2=RandomBG. Default 0 = follow equipped deck's leader skin.</summary>
    public int MyPageBgSelectType { get; set; }

    /// <summary>The single chosen MyPageBG cosmetic id, used when SelectType=CustomBG. 0 = none.</summary>
    public int MyPageBgId { get; set; }

    #region Owned

    public ViewerInfo Info { get; set; } = new ViewerInfo();

    public ViewerMissionData MissionData { get; set; } = new ViewerMissionData();

    public ViewerCurrency Currency { get; set; } = new ViewerCurrency();

    public List<ViewerClassData> Classes { get; set; } = new List<ViewerClassData>();

    #endregion

    #region Collection

    public List<ShadowverseDeckEntry> Decks { get; set; } = new List<ShadowverseDeckEntry>();

    public List<OwnedCardEntry> Cards { get; set; } = new List<OwnedCardEntry>();

    public List<LeaderSkinEntry> LeaderSkins { get; set; } = new List<LeaderSkinEntry>();

    public List<DegreeEntry> Degrees { get; set; } = new List<DegreeEntry>();

    public List<EmblemEntry> Emblems { get; set; } = new List<EmblemEntry>();

    public List<OwnedItemEntry> Items { get; set; } = new List<OwnedItemEntry>();

    public List<SleeveEntry> Sleeves { get; set; } = new List<SleeveEntry>();

    public List<MyPageBackgroundEntry> MyPageBackgrounds { get; set; } = new List<MyPageBackgroundEntry>();

    public List<ViewerPackOpenCount> PackOpenCounts { get; set; } = new List<ViewerPackOpenCount>();

    public List<ViewerPackStarterClass> PackStarterClasses { get; set; } = new List<ViewerPackStarterClass>();

    public List<ViewerFreePackClaim> FreePackClaims { get; set; } = new List<ViewerFreePackClaim>();

    public List<ViewerRankProgress> RankProgress { get; set; } = new List<ViewerRankProgress>();

    public List<MyPageBgRotationEntry> MyPageBgRotation { get; set; } = new List<MyPageBgRotationEntry>();

    public List<ViewerGachaPointBalance> GachaPointBalances { get; set; } = new List<ViewerGachaPointBalance>();

    public List<ViewerGachaPointReceived> GachaPointReceived { get; set; } = new List<ViewerGachaPointReceived>();

    public List<ViewerBuildDeckProductPurchase> BuildDeckPurchases { get; set; } = new List<ViewerBuildDeckProductPurchase>();

    public List<ViewerMission> Missions { get; set; } = new List<ViewerMission>();

    public List<ViewerAchievement> Achievements { get; set; } = new List<ViewerAchievement>();

    public List<ViewerEventCounter> EventCounters { get; set; } = new List<ViewerEventCounter>();

    #endregion

    #region Navigation Properties

    public List<SocialAccountConnection> SocialAccountConnections { get; set; } = new List<SocialAccountConnection>();

    public int? GuildId { get; set; }
    public SVSim.Database.Entities.Guild.Guild? Guild { get; set; }

    #endregion
}