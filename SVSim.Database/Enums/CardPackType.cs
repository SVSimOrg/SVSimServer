namespace SVSim.Database.Enums;

/// <summary>
/// Mirrors <c>GachaUI.CardPackType</c> in the decompiled client
/// (<c>Shadowverse_Code/GachaUI.cs</c> line 11). Wire value = (int)enum, carried on
/// /pack/info as <c>child_gacha_info[].type_detail</c>.
/// </summary>
public enum CardPackType
{
    None = 0,
    Crystal = 1,
    CrystalMulti = 2,
    Daily = 3,
    Ticket = 4,
    TicketMulti = 5,
    Rupy = 6,
    RupyMulti = 7,
    CrystalSpecial = 8,
    CrystalSelectSkin = 9,
    FreePacks = 10,
    FreePackWithSkin = 11,
    RotationStarterPack = 12,
    CrystalAcquireSkinCardPack = 13,
}
