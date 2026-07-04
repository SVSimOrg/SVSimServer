using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

/// <summary>Opponent-facing PlayActions frame the node synthesizes from the active player's
/// send. <c>KnownList</c> reveals the played card's identity (null = token reveal deferred, see
/// the deterministic-turn slice). <c>OppoTargetList</c> is the renamed <c>targetList</c>
/// (independent of KnownList — a targeted hand play carries both). <c>KeyAction</c> forwards a
/// choice/Discover play's <c>{type,cardId}</c> so the opponent renders the choice-token generation;
/// the pick (<c>selectCard</c>) is stripped for a hidden (open:0) draw-to-hand choice. <c>UList</c>
/// forwards the sender's unapproved-movement list (deck-sourced summons/fetches) verbatim. All are
/// omitted when null via the envelope's WhenWritingNull policy (a vanilla play carries none).</summary>
public sealed record PlayActionsBroadcastBody(
    [property: JsonPropertyName("playIdx")] int PlayIdx,
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("knownList")] IReadOnlyList<KnownCardEntry>? KnownList,
    [property: JsonPropertyName("oppoTargetList")] IReadOnlyList<OppoTargetEntry>? OppoTargetList,
    [property: JsonPropertyName("uList")] IReadOnlyList<UnapprovedCardEntry>? UList = null,
    [property: JsonPropertyName("keyAction")] IReadOnlyList<KeyActionEntry>? KeyAction = null) : IMsgBody;

/// <summary>Opponent-facing keyAction entry for a choice/Discover play. <c>type</c>/<c>cardId</c>
/// (the GENERATING card) pass through so the opponent re-derives the candidate pool from that card's
/// skill; <c>selectCard</c> is stripped (null) for a hidden (open:0) choice — the pick stays secret
/// until the chosen card is played — and passed through for a visible (open:1) board choice (§6,
/// provisional pending live confirmation).</summary>
public sealed record KeyActionEntry(
    [property: JsonPropertyName("type")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<KeyActionType>))] KeyActionType Type,
    [property: JsonPropertyName("cardId")] long CardId,
    [property: JsonPropertyName("selectCard")] SelectCardEntry? SelectCard);

/// <summary>A visible choice's revealed pick: the chosen <c>cardId</c>(s) and the <c>open</c> flag.
/// Only emitted for the open:1 pass-through case (open:0 strips the whole <c>selectCard</c>).</summary>
public sealed record SelectCardEntry(
    [property: JsonPropertyName("cardId")] IReadOnlyList<long> CardId,
    [property: JsonPropertyName("open")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<ChoiceVisibility>))] ChoiceVisibility Open);

/// <summary>One revealed card in a <c>knownList</c>. <c>cardId</c> from the sender's deck map; <c>cost</c>
/// is the ENGINE-RESOLVED play-time cost (M-HC-3a) — the discounted cost the headless engine actually
/// charged (spellboost + board modifiers folded in by construction), emitted on EVERY entry (prod sends
/// cost 45/45 in captures, so it is NOT omitted). <c>spellboost</c> is now ALSO engine-sourced (M-HC-3b) —
/// the played card's accumulated spell-charge count read straight off the resolved engine
/// (<c>SessionBattleEngine.PlayedCardSpellboost</c>); the wire-derived spellboost bookkeeping is retired.
/// Cost already folds the discount in by construction; the count rides the entry only to stay prod-faithful
/// (prod sends the real count).
/// <para><c>clan</c> and <c>tribe</c> are likewise ENGINE-SOURCED (M-HC-4e) — read off the resolved card's
/// <c>BattleCardBase.Clan</c>/<c>BattleCardBase.Tribe</c> getters (via
/// <c>SessionBattleEngine.PlayedCardClan</c>/<c>PlayedCardTribe</c>), which fold in
/// any skill-applied clan/tribe CHANGE/ADD (e.g. <c>change_affiliation</c>), so the wire carries the LIVE
/// clan/tribe the engine resolved, not the static card-master value. PROD ALWAYS EMITS BOTH on every
/// knownList entry (tk2 capture <c>battle-traffic_tk2_regular.ndjson</c>, e.g.
/// <c>{idx:17,cardId:128821011,...,clan:8,tribe:"7,16",...}</c>): <c>clan</c> is the int <c>ClanType</c>
/// ordinal (present even when 0); <c>tribe</c> is the comma-joined int <c>TribeType</c> ordinals as a
/// STRING, <c>"0"</c> when the card has no tribe (== <c>ClanType/TribeType.ALL == 0</c>, never empty/omitted —
/// the client reads it via <c>item.Value.ToString()</c>, NetworkBattleReceiver.cs:2382). Both are always
/// present (non-null string for tribe), so neither is null-omitted. attachTarget stays "".</para></summary>
public sealed record KnownCardEntry(
    [property: JsonPropertyName("idx")] int Idx,
    [property: JsonPropertyName("cardId")] long CardId,
    [property: JsonPropertyName("to")] int To,
    [property: JsonPropertyName("spellboost")] int Spellboost,
    [property: JsonPropertyName("attachTarget")] string AttachTarget,
    [property: JsonPropertyName("cost")] int Cost,
    [property: JsonPropertyName("clan")] int Clan,
    [property: JsonPropertyName("tribe")] string Tribe);

/// <summary>Renamed <c>targetList</c> entry. <c>isSelf</c> is actor-relative and passes through
/// verbatim — no perspective flip (bullet-3 audit F2).</summary>
public sealed record OppoTargetEntry(
    [property: JsonPropertyName("targetIdx")] int TargetIdx,
    [property: JsonPropertyName("isSelf")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<CardOwner>))] CardOwner IsSelf);

/// <summary>One entry in a relayed <c>uList</c> (the unapproved-movement list) — a skill-driven
/// card movement (fetch / search / summon-from-deck / discard-reveal) the node forwards VERBATIM
/// (bullet-3 audit F1; the node makes no reveal decision — <c>cardId</c> presence is the sender's
/// call). The first five fields are always emitted; the rest are conditional in
/// <c>SendCardDataMaker.MakeUList</c> (cardId when revealed, clan/cost when set, etc.) and omit when
/// null. <c>isSelf</c> is actor-relative and passes through unchanged (F2).</summary>
public sealed record UnapprovedCardEntry(
    [property: JsonPropertyName("idxList")] IReadOnlyList<int> IdxList,
    [property: JsonPropertyName("from")] int From,
    [property: JsonPropertyName("to")] int To,
    [property: JsonPropertyName("isSelf")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<CardOwner>))] CardOwner IsSelf,
    [property: JsonPropertyName("skill")] string Skill,
    [property: JsonPropertyName("cardId")] long? CardId = null,
    [property: JsonPropertyName("clan")] int? Clan = null,
    [property: JsonPropertyName("cost")] int? Cost = null,
    [property: JsonPropertyName("skillKeyCardIdx")] IReadOnlyList<int>? SkillKeyCardIdx = null,
    [property: JsonPropertyName("randomTargetIdx")] IReadOnlyList<int>? RandomTargetIdx = null,
    [property: JsonPropertyName("isInvoke")]
    [property: JsonConverter(typeof(NumericBoolJsonConverter))] bool? IsInvoke = null,
    [property: JsonPropertyName("attachTarget")] string? AttachTarget = null);
