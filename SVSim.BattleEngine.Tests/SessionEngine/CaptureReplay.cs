using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    internal sealed record CapturedFrame(DateTime Ts, string Direction, string Uri, MsgEnvelope Env, string RawBody);

    /// <summary>Parses a battle_test ndjson capture into MsgEnvelopes the engine can ingest.
    ///
    /// Capture quirk (verified against data_dumps/captures/battle_test): the authoritative URI lives at
    /// the TOP LEVEL for SEND frames (the body omits uri/viewerId/uuid and carries only the play
    /// payload) and in the BODY for RECEIVE frames (top-level uri is null). We resolve uri as
    /// top ?? body, then normalize the body into a full envelope (injecting the fields a send-frame body
    /// lacks) so MsgEnvelope.FromJson — which requires uri/viewerId/uuid — succeeds for both.</summary>
    internal static class CaptureReplay
    {
        public static IReadOnlyList<CapturedFrame> Load(string fixtureFileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureFileName);
            var frames = new List<CapturedFrame>();
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var direction = root.TryGetProperty("direction", out var dEl) ? dEl.GetString() ?? "" : "";
                var ts = root.TryGetProperty("ts", out var tsEl) && tsEl.ValueKind == JsonValueKind.String
                    ? DateTime.Parse(tsEl.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    : default;
                if (!root.TryGetProperty("body", out var bodyEl) || bodyEl.ValueKind != JsonValueKind.Object)
                    continue;

                string uri =
                    root.TryGetProperty("uri", out var tu) && tu.ValueKind == JsonValueKind.String
                        ? tu.GetString()!
                    : bodyEl.TryGetProperty("uri", out var bu) && bu.ValueKind == JsonValueKind.String
                        ? bu.GetString()!
                    : "None";

                // Normalize: send-frame bodies are bare payloads (no envelope fields). Inject the keys
                // FromJson requires; set the resolved uri.
                var obj = JsonNode.Parse(bodyEl.GetRawText())!.AsObject();
                obj["uri"] = uri;
                if (!obj.ContainsKey("viewerId")) obj["viewerId"] = 0L;
                if (!obj.ContainsKey("uuid")) obj["uuid"] = "";
                var normalized = obj.ToJsonString();

                MsgEnvelope env;
                try { env = MsgEnvelope.FromJson(normalized); }
                catch { continue; } // out-of-model / unparseable line
                frames.Add(new CapturedFrame(ts, direction, uri, env, normalized));
            }
            return frames;
        }

        /// <summary>Both clients' SENT frames interleaved in capture (ts) order, each tagged with its
        /// seat: cl1 == seat A == player (true), cl2 == seat B == opponent (false). This is the node's
        /// both-clients-sends ingest order — the same ts ordering the N1 shadow-replay test uses, here
        /// extended to merge both sides' sends rather than replaying one client's full receive stream.</summary>
        public static IEnumerable<(MsgEnvelope Env, bool Seat)> InterleavedSends(
            IReadOnlyList<CapturedFrame> cl1, IReadOnlyList<CapturedFrame> cl2)
        {
            return cl1.Where(f => f.Direction == "send").Select(f => (f, Seat: true))
                .Concat(cl2.Where(f => f.Direction == "send").Select(f => (f, Seat: false)))
                .OrderBy(x => x.f.Ts)
                .Select(x => (x.f.Env, x.Seat));
        }

        /// <summary>The selfDeck idx-&gt;cardId order from the Matched frame (the order the node also
        /// computed and handed the client). This is the deck the engine seats for that side.</summary>
        public static IReadOnlyList<long> SelfDeckFrom(IEnumerable<CapturedFrame> frames)
        {
            var matched = frames.FirstOrDefault(f => f.Uri == nameof(NetworkBattleUri.Matched));
            if (matched is null) return Array.Empty<long>();
            using var doc = JsonDocument.Parse(matched.RawBody);
            if (!doc.RootElement.TryGetProperty("selfDeck", out var deck)) return Array.Empty<long>();
            return deck.EnumerateArray()
                .OrderBy(e => e.GetProperty("idx").GetInt32())
                .Select(e => e.GetProperty("cardId").GetInt64())
                .ToList();
        }

        /// <summary>The per-battle master seed the capture carries (Matched.selfInfo.seed) — the seed the
        /// node generated and both clients used (F-N-5). Falls back to 0 if absent.</summary>
        public static int SeedFrom(IEnumerable<CapturedFrame> frames)
        {
            var matched = frames.FirstOrDefault(f => f.Uri == nameof(NetworkBattleUri.Matched));
            if (matched is null) return 0;
            using var doc = JsonDocument.Parse(matched.RawBody);
            if (doc.RootElement.TryGetProperty("selfInfo", out var si)
                && si.TryGetProperty("seed", out var seed)
                && seed.TryGetInt32(out var v))
                return v;
            return 0;
        }
    }
}
