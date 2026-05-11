using UnityEngine;

namespace SuperRodentGame.Core
{
    public enum Difficulty { Easy, Normal, Hard }

    public static class DifficultySettings
    {
        private const string PrefKey = "settings.difficulty";

        // ── Пресеты ────────────────────────────────────────────────────────
        public static readonly float[] EnemySpeedMult  = { 0.7f, 1.0f, 1.5f };
        public static readonly float[] EnemyHealthMult = { 0.7f, 1.0f, 1.5f };
        public static readonly float[] AutoStartDelay  = { 20f,  15f,  8f   };
        public static readonly float[] CoreHp          = { 20f,  10f,  1f   };

        private static Difficulty? _current;

        public static Difficulty Current
        {
            get
            {
                if (_current == null)
                    _current = (Difficulty)PlayerPrefs.GetInt(PrefKey, (int)Difficulty.Normal);
                return _current.Value;
            }
            set
            {
                _current = value;
                PlayerPrefs.SetInt(PrefKey, (int)value);
            }
        }

        public static float SpeedMult   => EnemySpeedMult [(int)Current];
        public static float HealthMult  => EnemyHealthMult[(int)Current];
        public static float StartDelay  => AutoStartDelay [(int)Current];
        public static float StartCoreHp => CoreHp         [(int)Current];
    }
}
