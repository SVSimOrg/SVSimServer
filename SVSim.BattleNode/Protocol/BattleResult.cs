namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wire value of <c>result</c> on a WS <c>BattleFinish</c> frame.
/// <para>
/// Maps to the client's <c>NetworkBattleReceiver.RESULT_CODE</c> enum at
/// <c>NetworkBattleReceiver.cs:963-986</c>. Names are <b>from the player's perspective</b>:
/// <c>LifeWin</c> = "I won by life", <c>LifeLose</c> = "I lost by life". Verified
/// end-to-end via the path
/// <c>RESULT_CODE</c> → <c>JudgeResultReceive</c> switch → <c>_finishEffectType</c> →
/// <c>FinishBattleEffect</c> → <c>InitiateGameEndSequence(hasWon)</c>:
/// </para>
/// <list type="bullet">
///   <item><c>LifeWin = 101</c> → <c>_finishEffectType = LOSE</c> →
///         <c>InitiateGameEndSequence(hasWon: true)</c> → PLAYER WIN UI</item>
///   <item><c>LifeLose = 102</c> → <c>_finishEffectType = WIN</c> →
///         <c>InitiateGameEndSequence(hasWon: false)</c> → PLAYER LOSE UI</item>
/// </list>
/// <para>
/// The <c>SettingResultUI_SpecialResultTypeText</c> switch passes the OPPONENT's
/// outcome to set the secondary "by retire/by disconnect/etc." text — that's why
/// the inner switch direction looks inverted (LifeWin → LOSE param, LifeLose → WIN
/// param). The actual WIN/LOSE rendering happens in <c>FinishBattleEffect</c> via
/// the <c>!isPlayer</c> flip at line 1315.
/// </para>
/// <para>
/// Prior docstrings on this enum had the direction backwards (claimed LifeLose
/// → WIN UI from a misread of the inner switch); see
/// <c>docs/audits/battle-node-sio-events-2026-06-02.md</c> Addendum for the live
/// reproduction that exposed the inversion.
/// </para>
/// <para>
/// Always serialize as the int value, not the name; see the
/// <c>JsonNumberEnumConverter</c> on <see cref="Bodies.BattleFinishBody.Result"/>.
/// </para>
/// </summary>
public enum BattleResult
{
    /// <summary>Player won by reducing opponent's life to 0. Pushed to the winner
    /// on <c>TurnEndFinal</c>. Routes through the client switch to
    /// <c>InitiateGameEndSequence(hasWon: true)</c>.</summary>
    LifeWin = 101,

    /// <summary>Player lost by their own life dropping to 0. Pushed to the loser on
    /// the opponent's <c>TurnEndFinal</c>. Prod TK2 capture at
    /// <c>data_dumps/captures/battle-traffic_tk2_regular.ndjson:274</c> is a loss
    /// shown to the player from a real opponent's lethal.</summary>
    LifeLose = 102,

    /// <summary>Player won because opponent retired. Pushed to the survivor on
    /// <c>Retire</c>/<c>Kill</c>. Same player-perspective convention as the Life codes.</summary>
    RetireWin = 105,

    /// <summary>Player lost by retiring. Pushed to the retirer on <c>Retire</c>/<c>Kill</c>.</summary>
    RetireLose = 106,

    /// <summary>Survivor wins because the opponent's socket dropped without a graceful
    /// <c>Retire</c>. Pushed to the survivor by the <see cref="Sessions.BattleSession"/>
    /// RunAsync drop cascade. Client <c>RESULT_CODE.DisconnectWin</c> renders the
    /// "opponent disconnected" result text → player WIN UI. Same player-perspective
    /// convention as the Life/Retire codes.</summary>
    DisconnectWin = 201,
}
