using UnityEngine;

namespace Squishies
{
    public class ComboSystem : MonoBehaviour
    {
        public static ComboSystem Instance { get; private set; }

        public int CurrentCombo { get; private set; }

        private float comboTimer;
        private const float COMBO_TIMEOUT = 2.5f;

        public event System.Action<int> OnComboChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (CurrentCombo > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        public void RegisterMatch(int chainLength)
        {
            CurrentCombo++;
            comboTimer = COMBO_TIMEOUT;
            OnComboChanged?.Invoke(CurrentCombo);
        }

        public bool ShouldCreateChonky(int matchLength)
        {
            return matchLength >= 5 && matchLength < 8;
        }

        public bool ShouldCreateMegaChonk(int matchLength)
        {
            return matchLength >= 8;
        }

        /// <summary>
        /// Returns combo level as int:
        /// 0 = no combo, 1 = "Nice", 2 = "Great", 3 = "Amazing", 4+ = "INCREDIBLE"
        /// Text mapping is done by UI layer.
        /// </summary>
        public int GetComboLevel()
        {
            if (CurrentCombo <= 0)
                return 0;

            return Mathf.Min(CurrentCombo, 4);
        }

        public void ResetCombo()
        {
            CurrentCombo = 0;
            comboTimer = 0f;
            OnComboChanged?.Invoke(CurrentCombo);
        }
    }
}
