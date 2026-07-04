namespace SVSim.Database.Models;

/// <summary>
/// One row in the tutorial-gift catalogue every fresh viewer is given at signup. Authored in
/// <c>SVSim.Bootstrap/Data/seeds/tutorial-presents.json</c>; <see cref="PresentId"/> is the
/// wire-stable identifier and serves as the primary key. <c>ViewerRepository.RegisterAnonymousViewer</c>
/// reads this table and projects each row into a <see cref="ViewerPresent"/> with Source="tutorial".
/// </summary>
public class TutorialPresentEntry
{
    public string PresentId { get; set; } = string.Empty;

    public int RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public long RewardCount { get; set; }
    public int? ItemType { get; set; }
    public string Message { get; set; } = string.Empty;
}
