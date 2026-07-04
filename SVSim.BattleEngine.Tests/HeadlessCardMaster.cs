using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Wizard;

namespace SVSim.BattleEngine.Tests
{
    // Populates the engine's static CardMaster headless, from the loader's cards.json dump
    // (serialized CardCSVData objects). We bypass the network/Resources init path
    // (CardMaster.InitializeCardMaster) and the private ctor/field via reflection — CardMaster
    // exposes no public injection seam. Class cards (id < 100) resolve via the ctor's
    // _classCardParam, so an empty load still satisfies construction; pass real ids for the oracle.
    public static class HeadlessCardMaster
    {
        private static readonly string CardsJsonPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "cards.json");

        // Every id ever requested this process. Load is CUMULATIVE: each call rebuilds the master from
        // the union, so a later Load(subset) never evicts cards an earlier Load (e.g. EnsureProcessGlobals's
        // oracle set) installed. Without this, the static CardMaster is shared mutable state across the
        // whole NUnit run and a Load(deck) in one test silently breaks an oracle test that runs after.
        private static readonly HashSet<int> _everLoaded = new();
        // Serialise Load: assembly-level Parallelizable(Fixtures) means concurrent fixtures race here,
        // and HashSet<int>.Add + the static CardMaster install are not thread-safe.
        private static readonly object _loadGate = new object();

        // Load the given card ids (empty = none) into a CardMaster registered as Default, MERGED with all
        // previously-loaded ids.
        public static void Load(params int[] cardIds)
        {
            lock (_loadGate)
            {
                LoadCore(cardIds);
            }
        }

        private static void LoadCore(int[] cardIds)
        {
            foreach (var id in cardIds) _everLoaded.Add(id);
            var want = new HashSet<int>(_everLoaded);
            var rows = new List<CardCSVData>();
            if (want.Count > 0)
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(CardsJsonPath));
                int sort = 0;
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (!el.TryGetProperty("card_id", out var idEl)) continue;
                    if (!int.TryParse(idEl.GetString(), out var id) || !want.Contains(id)) continue;
                    rows.Add(BuildCardCsvData(el, sort++));
                }
                var missing = want.Except(rows.Select(r => int.Parse(r.card_id))).ToArray();
                if (missing.Length > 0)
                    throw new InvalidOperationException(
                        "cards.json missing requested ids: " + string.Join(",", missing));
            }

            var cm = NewCardMaster(rows);
            InjectAsDefault(cm);
        }

        // Construct a CardCSVData without running its CSV ctor; set each member from the JSON object
        // by exact name match (cards.json keys == CardCSVData member names).
        private static CardCSVData BuildCardCsvData(JsonElement el, int sortIndex)
        {
            var c = (CardCSVData)FormatterServices.GetUninitializedObject(typeof(CardCSVData));
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var prop in el.EnumerateObject())
            {
                string val = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.ToString();
                var f = typeof(CardCSVData).GetField(prop.Name, bf);
                if (f != null) { SetMember(f.FieldType, val, v => f.SetValue(c, v)); continue; }
                var p = typeof(CardCSVData).GetProperty(prop.Name, bf);
                if (p != null && p.CanWrite) SetMember(p.PropertyType, val, v => p.SetValue(c, v));
            }
            // SortIndex is normally set by the ctor; mirror it.
            var si = typeof(CardCSVData).GetProperty("SortIndex", bf);
            if (si != null && si.CanWrite) si.SetValue(c, sortIndex);
            return c;
        }

        private static void SetMember(Type t, string val, Action<object> set)
        {
            if (t == typeof(string)) set(val);
            else if (t == typeof(int)) set(int.TryParse(val, out var i) ? i : 0);
            else if (t == typeof(bool)) set(val == "1" || string.Equals(val, "true", StringComparison.OrdinalIgnoreCase));
            // other types left at default
        }

        private static CardMaster NewCardMaster(List<CardCSVData> rows)
        {
            var ctor = typeof(CardMaster).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(List<CardCSVData>) }, null);
            if (ctor == null) throw new InvalidOperationException("CardMaster(List<CardCSVData>) ctor not found");
            return (CardMaster)ctor.Invoke(new object[] { rows });
        }

        private static void InjectAsDefault(CardMaster cm)
        {
            var idType = typeof(CardMaster).GetNestedType("CardMasterId");
            var defaultId = Enum.Parse(idType, "Default");
            var dictType = typeof(Dictionary<,>).MakeGenericType(idType, typeof(CardMaster));
            var dict = (System.Collections.IDictionary)Activator.CreateInstance(dictType);
            dict[defaultId] = cm;
            var fld = typeof(CardMaster).GetField("_dictCardMaster",
                BindingFlags.Static | BindingFlags.NonPublic);
            fld.SetValue(null, dict);
        }
    }
}
