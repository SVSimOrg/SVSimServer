using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserCurrency
{
    [JsonPropertyName("viewer_id")]
    [Key("viewer_id")]
    public long ViewerId { get; set; }
    [JsonPropertyName("crystal")]
    [Key("crystal")]
    public ulong Crystals { get; set; }
    [JsonPropertyName("crystal_android")]
    [Key("crystal_android")]
    public ulong AndroidCrystals { get; set; }
    [JsonPropertyName("crystal_ios")]
    [Key("crystal_ios")]
    public ulong IosCrystals { get; set; }
    [JsonPropertyName("crystal_steam")]
    [Key("crystal_steam")]
    public ulong SteamCrystals { get; set; }
    [JsonPropertyName("crystal_dmm")]
    [Key("crystal_dmm")]
    public ulong DmmCrystals { get; set; }
    [JsonPropertyName("free_crystal")]
    [Key("free_crystal")]
    public ulong FreeCrystals { get; set; }
    [JsonPropertyName("total_crystal")]
    [Key("total_crystal")]
    public ulong TotalCrystals { get; set; }
    [JsonPropertyName("life_total_crystal")]
    [Key("life_total_crystal")]
    public ulong LifeTotalCrystals { get; set; }
    [JsonPropertyName("red_ether")]
    [Key("red_ether")]
    public ulong RedEther { get; set; }
    [JsonPropertyName("rupy")]
    [Key("rupy")]
    public ulong Rupees { get; set; }

    public UserCurrency()
    {
        
    }
    
    public UserCurrency(Viewer viewer)
    {
        ViewerCurrency currency = viewer.Currency;
        this.Crystals = currency.Crystals;
        this.RedEther = currency.RedEther;
        this.LifeTotalCrystals = currency.LifeTotalCrystals;
        this.TotalCrystals = currency.Crystals;
        this.Rupees = currency.Rupees;
        this.FreeCrystals = currency.FreeCrystals;
        this.AndroidCrystals = currency.AndroidCrystals;
        this.DmmCrystals = currency.DmmCrystals;
        this.SteamCrystals = currency.SteamCrystals;
        this.IosCrystals = currency.IosCrystals;
        this.ViewerId = viewer.Id;
    }
}