using System.Collections.Generic;
using UnityEngine;

namespace Squishies
{
    public static class SquishyDatabase
    {
        private static List<SquishyData> _allTypes;

        public static List<SquishyData> AllTypes
        {
            get
            {
                if (_allTypes == null)
                    Initialize();
                return _allTypes;
            }
        }

        public static SquishyData GetByType(SquishyType type)
        {
            return AllTypes.Find(d => d.squishyType == type);
        }

        private static void Initialize()
        {
            _allTypes = new List<SquishyData>();
            Add(SquishyType.Bloop, "Bloop", new Color(0.40f, 0.60f, 1.00f), MegaChonkAbility.RadialBurst);
            Add(SquishyType.Rosie, "Rosie", new Color(1.00f, 0.60f, 0.75f), MegaChonkAbility.RowClear);
            Add(SquishyType.Limbo, "Limbo", new Color(0.50f, 0.85f, 0.50f), MegaChonkAbility.ColumnClear);
            Add(SquishyType.Sunny, "Sunny", new Color(1.00f, 0.90f, 0.40f), MegaChonkAbility.ColorDrain);
            Add(SquishyType.Plum, "Plum", new Color(0.70f, 0.50f, 0.90f), MegaChonkAbility.Shuffle);
            Add(SquishyType.Tangy, "Tangy", new Color(1.00f, 0.70f, 0.30f), MegaChonkAbility.HappinessBurst);
            Add(SquishyType.Mochi, "Mochi", new Color(0.95f, 0.95f, 0.95f), MegaChonkAbility.Wildcard);
        }

        private static void Add(SquishyType type, string name, Color color, MegaChonkAbility ability)
        {
            var data = ScriptableObject.CreateInstance<SquishyData>();
            data.squishyType = type;
            data.displayName = name;
            data.color = color;
            data.megaAbility = ability;
            data.name = name;
            _allTypes.Add(data);
        }
    }
}
