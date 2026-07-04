using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;

namespace SVSim.BattleNode.Hosting;

/// <summary>
/// Registration + pipeline extensions that turn an arbitrary ASP.NET Core host into a battle
/// node. The library has no dependency on any specific host project — call both methods from
/// wherever you build your <see cref="WebApplication"/>.
/// </summary>
public static class BattleNodeExtensions
{
    /// <summary>
    /// Register the battle node's services in DI. All four are singletons because none of them
    /// carry per-request state — per-battle state lives on the <see cref="BattleSession"/>
    /// instance the WebSocket handler constructs on connect.
    /// </summary>
    /// <param name="configure">
    /// Callback to populate <see cref="BattleNodeOptions"/>. Must set
    /// <see cref="BattleNodeOptions.NodeServerUrl"/> — there is no hardcoded fallback; the
    /// value is a deployment concern (localhost during dev, a real host[:port]/socket.io/
    /// when the node runs behind a reverse proxy or on a separate box). Startup throws
    /// <see cref="InvalidOperationException"/> if it's still empty after the callback runs.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="BattleNodeOptions.NodeServerUrl"/> is null, empty, or whitespace after
    /// <paramref name="configure"/> runs.
    /// </exception>
    public static IServiceCollection AddBattleNode(this IServiceCollection services, Action<BattleNodeOptions>? configure = null)
    {
        var options = new BattleNodeOptions();
        configure?.Invoke(options);
        if (string.IsNullOrWhiteSpace(options.NodeServerUrl))
        {
            throw new InvalidOperationException(
                "BattleNode:NodeServerUrl is not configured. Set it in appsettings.json under " +
                "the \"BattleNode\" section (format: \"host[:port]/socket.io/\", no scheme prefix).");
        }
        services.AddSingleton(options);
        services.AddSingleton<IBattleSessionStore, InMemoryBattleSessionStore>();
        services.AddSingleton<IMatchingBridge, MatchingBridge>();
        services.AddSingleton<IWaitingRoom, WaitingRoom>();
        services.AddSingleton<BattleNodeWebSocketHandler>();
        return services;
    }

    /// <summary>
    /// Wire up the WebSocket middleware and map the Socket.IO endpoint at <c>/socket.io/</c>.
    /// Call this AFTER any HTTP middleware that should still see non-WS requests (auth,
    /// routing, controllers) and BEFORE <c>MapControllers()</c>. The endpoint accepts any
    /// path under <c>/socket.io</c>; the handler doesn't read the sub-path, so default
    /// Socket.IO clients targeting <c>/socket.io/?EIO=3&amp;transport=websocket</c> work
    /// without configuration.
    /// </summary>
    /// <remarks>
    /// Steam auth gets a free pass on WS upgrades — see
    /// <c>SteamSessionAuthenticationHandler</c>'s header-based bypass. The node has its own
    /// per-connection auth (encrypted viewerId in the upgrade headers, validated against the
    /// matched battle id in <see cref="BattleNodeWebSocketHandler.HandleAsync"/>).
    /// </remarks>
    public static IApplicationBuilder UseBattleNode(this IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.Map("/socket.io", branch => branch.Run(HandleSocketIoAsync));
        return app;
    }

    /// <summary>
    /// Terminal handler for <c>/socket.io/*</c> — resolves the singleton
    /// <see cref="BattleNodeWebSocketHandler"/> from DI and hands the request over.
    /// Extracted from the inline lambda in <see cref="UseBattleNode"/> so stack traces
    /// show a real method name during WS connect failures.
    /// </summary>
    private static async Task HandleSocketIoAsync(Microsoft.AspNetCore.Http.HttpContext ctx)
    {
        var handler = ctx.RequestServices.GetRequiredService<BattleNodeWebSocketHandler>();
        await handler.HandleAsync(ctx);
    }
}
