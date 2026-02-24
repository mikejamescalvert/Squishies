using UnityEngine;

namespace Squishies
{
    public enum SquishyType { Bloop, Rosie, Limbo, Sunny, Plum, Tangy, Mochi }
    public enum MegaChonkAbility { RadialBurst, RowClear, ColumnClear, ColorDrain, Shuffle, HappinessBurst, Wildcard }

    [CreateAssetMenu(fileName = "NewSquishyType", menuName = "Squishies/Squishy Type")]
    public class SquishyData : ScriptableObject
    {
        public SquishyType squishyType;
        public Color color;
        public MegaChonkAbility megaAbility;
        public string displayName;
    }
}
