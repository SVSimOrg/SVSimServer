using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserItem
{
    [JsonPropertyName("item_id")]
    [Key("item_id")]
    public int ItemId { get; set; }
    
    [JsonPropertyName("number")]
    [Key("number")]
    public int Number { get; set; }

    public UserItem(OwnedItemEntry item)
    {
        this.ItemId = item.Item.Id;
        this.Number = item.Count;
    }

    public UserItem()
    {
    }
}