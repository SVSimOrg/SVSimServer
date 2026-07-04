namespace SVSim.Database.Entities.Guild;

public enum GuildRole : int
{
    Regular = 0,
    Leader = 1,
    SubLeader = 2,
}

public enum GuildJoinCondition : int
{
    Free = 1,
    Approval = 2,
    OnlyInvite = 3,
}

// Matches GuildDetailInfo.ActivityType (1..16). All / Beginner / Enjoy / Stoic / FriendOnly /
// per-class (Elf..Nemesis) / Rotation / Unlimited / TwoPick.
public enum GuildActivity : int
{
    All = 1, Beginner = 2, Enjoy = 3, Stoic = 4, FriendOnly = 5,
    Elf = 6, Royal = 7, Witch = 8, Dragon = 9, Necro = 10,
    Vampire = 11, Bishop = 12, Nemesis = 13,
    Rotation = 14, Unlimited = 15, TwoPick = 16,
}

public enum GuildInviteStatus : int { Pending = 0, Canceled = 1, Rejected = 2, Consumed = 3 }
public enum GuildJoinRequestStatus : int { Pending = 0, Canceled = 1, Rejected = 2, Accepted = 3 }

// Per docs/api-spec/.../guild_chat-messages.md. 12..18 are gathering-only — never emit on guild.
public enum GuildChatMessageType : int
{
    Normal = 0, Stamp = 1, Deck = 2, Join = 3, Leave = 4, Replay = 5,
    ChangeLeader = 6, ChangeSubLeader = 7, CreateGuild = 8, Remove = 9,
    RoomMatch = 10, Description = 11,
}
