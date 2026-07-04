namespace SVSim.Database.Models.Config;

[ConfigSection("Player")]
public class PlayerConfig
{
    public int MaxFriends { get; set; } = 20;

    public static PlayerConfig ShippedDefaults() => new();
}
