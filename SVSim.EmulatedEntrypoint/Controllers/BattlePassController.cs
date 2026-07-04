using Microsoft.AspNetCore.Mvc;
using SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /battle_pass/* — season metadata, premium-pass purchase. Wire shapes mirror
/// Wizard/BattlePass{Info,PurchaseInfo,Buy}Task.cs.
/// </summary>
[Route("battle_pass")]
public class BattlePassController : SVSimController
{
    private readonly IBattlePassService _battlePass;

    public BattlePassController(IBattlePassService battlePass)
    {
        _battlePass = battlePass;
    }

    [HttpPost("info")]
    public async Task<IActionResult> Info(BaseRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var info = await _battlePass.GetInfoAsync(viewerId, ct);
        // TODO(off-season-crash): Empty {} body crashes BattlePassInfoTask.Parse() on the
        // client (unconditional jsonData["season_info"] access). Unreachable in practice —
        // season 23 outlasts the Cygames shutdown. If a season-24+ ever lands, set
        // data_headers.result_code != 1 here so base.Parse() short-circuits before the
        // subclass Parse runs.
        if (info is null) return Ok(new { });
        return Ok(info);
    }

    [HttpPost("item_list")]
    public async Task<IActionResult> ItemList(BaseRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var list = await _battlePass.GetItemListAsync(viewerId, ct);
        // TODO(off-season-crash): Empty {} body crashes BattlePassPurchaseInfoTask.Parse() on the
        // client (unconditional jsonData["premium_pass_description"] access). Unreachable in
        // practice — season 23 outlasts the Cygames shutdown. If a season-24+ ever lands, set
        // data_headers.result_code != 1 here so base.Parse() short-circuits before the
        // subclass Parse runs.
        if (list is null) return Ok(new { });
        return Ok(list);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy(BattlePassBuyRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var outcome = await _battlePass.BuyPremiumAsync(viewerId, request.SeasonId, request.Id, ct);

        var response = new BattlePassBuyResponse
        {
            ResultCode = outcome.ResultCode,
            AchievedInfo = new BattlePassAchievedInfoDto
            {
                BattlePassRewardList = outcome.AchievedRewards
                    .Select(g => new BattlePassReceivedRewardDto
                    {
                        RewardType = (int)g.RewardType,
                        RewardDetailId = g.RewardId,
                        RewardNumber = g.RewardNum,
                    }).ToList(),
            },
            RewardList = outcome.PostStateTotals
                .Select(g => new BattlePassRewardListEntryDto
                {
                    RewardType = (int)g.RewardType,
                    RewardId = g.RewardId,
                    RewardNum = g.RewardNum,
                }).ToList(),
        };
        return Ok(response);
    }
}
