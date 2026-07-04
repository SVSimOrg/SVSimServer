using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Services.Friend;
using SVSim.EmulatedEntrypoint.Models.Dtos.Friend;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /friend/* — viewer-scoped friend system. 5 reads + 7 writes. All writes are
/// "silent rejection" on failure (cap exceeded, not addressed to caller, etc.) — the client
/// pass-through Parse()s don't differentiate.
/// </summary>
[Route("friend")]
public sealed class FriendController : SVSimController
{
    private readonly IFriendService _friend;

    public FriendController(IFriendService friend) => _friend = friend;

    [HttpPost("info")]
    public async Task<ActionResult<FriendInfoResponse>> Info([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var result = await _friend.GetFriendsAsync(viewerId, ct);
        return new FriendInfoResponse
        {
            Friends = result.Friends.Select(ToWire).ToList(),
            FriendCount = result.Count,
            FriendMaxCount = result.MaxCount,
        };
    }

    [HttpPost("receive_apply_info")]
    public async Task<ActionResult<ReceiveApplyInfoResponse>> ReceiveApplyInfo([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var result = await _friend.GetReceiveAppliesAsync(viewerId, ct);
        return new ReceiveApplyInfoResponse
        {
            ReceiveApplies = result.ReceiveApplies.Select(ToWire).ToList(),
            ApproveApplyCount = result.ApproveApplyCount,
        };
    }

    [HttpPost("send_apply_info")]
    public async Task<ActionResult<SendApplyInfoResponse>> SendApplyInfo([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var result = await _friend.GetSendAppliesAsync(viewerId, ct);
        return new SendApplyInfoResponse
        {
            SendApplies = result.SendApplies.Select(ToWire).ToList(),
            RemainingApplyCount = result.RemainingApplyCount,
            SendApplyMaxCount = result.SendApplyMaxCount,
        };
    }

    [HttpPost("played_together_info")]
    public async Task<ActionResult<PlayedTogetherInfoResponse>> PlayedTogetherInfo([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var result = await _friend.GetPlayedTogetherAsync(viewerId, ct);
        return new PlayedTogetherInfoResponse
        {
            Histories = result.Histories.Select(ToWire).ToList(),
        };
    }

    [HttpPost("search_user")]
    public async Task<ActionResult<SearchUserResponse>> SearchUser([FromBody] SearchUserRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var hit = await _friend.SearchAsync(viewerId, req.SearchViewerId, ct);
        return new SearchUserResponse
        {
            UserInfo = hit is null ? new object() : ToWire(hit),
        };
    }

    [HttpPost("send_apply")]
    public async Task<IActionResult> SendApply([FromBody] SendApplyRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.SendApplyAsync(viewerId, req.FriendId, ct);
        return Ok(new { });
    }

    [HttpPost("approve_apply")]
    public async Task<IActionResult> ApproveApply([FromBody] ApplyIdRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.ApproveApplyAsync(viewerId, req.ApplyId, ct);
        return Ok(new { });
    }

    [HttpPost("reject_apply")]
    public async Task<IActionResult> RejectApply([FromBody] ApplyIdRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.RejectApplyAsync(viewerId, req.ApplyId, ct);
        return Ok(new { });
    }

    [HttpPost("cancel_apply")]
    public async Task<IActionResult> CancelApply([FromBody] ApplyIdRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.CancelApplyAsync(viewerId, req.ApplyId, ct);
        return Ok(new { });
    }

    [HttpPost("reject_apply_all")]
    public async Task<IActionResult> RejectApplyAll([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.RejectAllAppliesAsync(viewerId, ct);
        return Ok(new { });
    }

    [HttpPost("cancel_apply_all")]
    public async Task<IActionResult> CancelApplyAll([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.CancelAllAppliesAsync(viewerId, ct);
        return Ok(new { });
    }

    [HttpPost("reject_friend")]
    public async Task<IActionResult> RejectFriend([FromBody] RejectFriendRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        await _friend.RejectFriendAsync(viewerId, req.FriendId, ct);
        return Ok(new { });
    }

    private static FriendEntryDto ToWire(FriendEntry e) => new()
    {
        DeviceType = e.DeviceType,
        Name = e.Name,
        CountryCode = e.CountryCode,
        MaxFriend = e.MaxFriend,
        LastPlayTime = e.LastPlayTime,
        IsReceivedTwoPickMission = e.IsReceivedTwoPickMission,
        Birth = e.Birth,
        MissionChangeTime = e.MissionChangeTime,
        MissionReceiveType = e.MissionReceiveType,
        IsOfficial = e.IsOfficial,
        IsOfficialMarkDisplayed = e.IsOfficialMarkDisplayed,
        ViewerId = e.ViewerId,
        Rank = e.Rank,
        EmblemId = e.EmblemId,
        DegreeId = e.DegreeId,
    };

    private static FriendApplyEntryDto ToWire(FriendApplyEntry e) => new()
    {
        Id = e.Id,
        ViewerId = e.ViewerId,
        Name = e.Name,
        CountryCode = e.CountryCode,
        Rank = e.Rank,
        EmblemId = e.EmblemId,
        DegreeId = e.DegreeId,
        LastPlayTime = e.LastPlayTime,
        CreateTime = e.CreateTime,
        MissionType = e.MissionType,
    };

    private static PlayedTogetherEntryDto ToWire(PlayedTogetherEntry e) => new()
    {
        ViewerId = e.ViewerId,
        Name = e.Name,
        CountryCode = e.CountryCode,
        Rank = e.Rank,
        EmblemId = e.EmblemId,
        DegreeId = e.DegreeId,
        LastPlayTime = e.LastPlayTime,
        PlayedTime = e.PlayedTime,
        FriendStatus = e.FriendStatus,
        FriendApplyId = e.FriendApplyId,
        PlayedMode = e.PlayedMode,
        BattleType = e.BattleType,
        DeckFormat = e.DeckFormat,
        TwoPickType = e.TwoPickType,
    };
}
