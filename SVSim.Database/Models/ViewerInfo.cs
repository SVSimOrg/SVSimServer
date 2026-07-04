using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class ViewerInfo
{
    public DateTime BirthDate { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public int MaxFriends { get; set; }
    public bool IsOfficial { get; set; }
    public bool IsOfficialMarkDisplayed { get; set; }
    public bool IsFoilPreferred { get; set; }
    public bool IsPrizePreferred { get; set; }
    public bool IsSkipGachaEffect { get; set; }
    public bool UseChallengeTwoPickPremiumCard { get; set; }
    public long ChallengeTwoPickSleeveId { get; set; }

    #region Navigation Properties

    public EmblemEntry SelectedEmblem { get; set; } = new EmblemEntry();

    public DegreeEntry SelectedDegree { get; set; } = new DegreeEntry();

    #endregion
}