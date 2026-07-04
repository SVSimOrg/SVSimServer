using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Wizard;

namespace SVSim.BattleEngine.Tests
{
    // Builds the minimal Data.Master reference context a headless battle reads. In the client this
    // comes from the /load/index master section; here we author just enough for the resolution path
    // (currently: ClassCharacterList, so the leader/class card can resolve player/enemy class_id).
    // Entries are constructed without their CSV ctor (private setters set via reflection).
    public static class HeadlessMasterData
    {
        public const int PlayerCharaId = 1;
        public const int EnemyCharaId = 2;
        public const int PlayerClassId = 1; // ClanType -> class card clan
        public const int EnemyClassId = 2;

        public static void Install()
        {
            var master = (Master)FormatterServices.GetUninitializedObject(typeof(Master));
            // The resolution path reads many Master.* collections (e.g. WhenPlayEffectKeywordMaster)
            // and calls LINQ on them unguarded. Default every collection member to an empty instance
            // so those touches no-op instead of NRE; then override the ones we need with content.
            EnsureEmptyCollections(master);
            var list = new List<ClassCharacterMasterData>
            {
                NewChara(PlayerCharaId, PlayerClassId),
                NewChara(EnemyCharaId, EnemyClassId),
            };
            SetMember(master, "ClassCharacterList", list);
            Data.Master = master;
        }

        // Initialize every List<>/array/Dictionary<> field/auto-property on the object to an empty
        // non-null instance (only if currently null).
        private static void EnsureEmptyCollections(object obj)
        {
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var f in obj.GetType().GetFields(bf))
            {
                if (f.GetValue(obj) != null) continue;
                var empty = EmptyOf(f.FieldType);
                if (empty != null) f.SetValue(obj, empty);
            }
        }

        private static object EmptyOf(Type t)
        {
            if (t.IsArray) return Array.CreateInstance(t.GetElementType(), 0);
            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                if (def == typeof(List<>) || def == typeof(Dictionary<,>) ||
                    def == typeof(HashSet<>) || def == typeof(IList<>) ||
                    def == typeof(IDictionary<,>) || def == typeof(ICollection<>) ||
                    def == typeof(IEnumerable<>))
                {
                    var concrete = def == typeof(List<>) || def == typeof(IList<>) ||
                                   def == typeof(ICollection<>) || def == typeof(IEnumerable<>)
                        ? typeof(List<>).MakeGenericType(t.GetGenericArguments())
                        : def == typeof(HashSet<>)
                            ? typeof(HashSet<>).MakeGenericType(t.GetGenericArguments())
                            : typeof(Dictionary<,>).MakeGenericType(t.GetGenericArguments());
                    return Activator.CreateInstance(concrete);
                }
            }
            return null;
        }

        private static ClassCharacterMasterData NewChara(int charaId, int classId)
        {
            var c = (ClassCharacterMasterData)FormatterServices.GetUninitializedObject(typeof(ClassCharacterMasterData));
            SetMember(c, "chara_id", charaId);
            SetMember(c, "class_id", classId);
            SetMember(c, "skin_id", charaId);
            SetMember(c, "is_usable", true);
            return c;
        }

        // Set a member (auto-property backing field or field) by name, tolerating private setters.
        private static void SetMember(object obj, string name, object value)
        {
            var t = obj.GetType();
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var p = t.GetProperty(name, bf);
            if (p != null && p.SetMethod != null) { p.SetValue(obj, value); return; }
            var f = t.GetField(name, bf)
                    ?? t.GetField($"<{name}>k__BackingField", bf);
            if (f != null) { f.SetValue(obj, value); return; }
            throw new InvalidOperationException($"{t.Name} has no settable member '{name}'");
        }
    }
}
