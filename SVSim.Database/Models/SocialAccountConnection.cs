using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// A connection between a social account (ie facebook) and a viewer.
/// </summary>
[Owned]
public class SocialAccountConnection
{
    /// <summary>
    /// The type of the social account.
    /// </summary>
    public SocialAccountType AccountType { get; set; }
    
    /// <summary>
    /// The identifier of the social account.
    /// </summary>
    public ulong AccountId { get; set; }

    /// <summary>
    /// The viewer connected.
    /// </summary>
    public Viewer Viewer { get; set; } = new Viewer();
}