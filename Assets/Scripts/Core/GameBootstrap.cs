using SuperRodentGame.Combat;
using SuperRodentGame.UI;
using UnityEngine;

namespace SuperRodentGame.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Optional scene references")]
        [SerializeField] private Audio.AudioManager audioManager;
        [SerializeField] private SettingsMenuController settingsMenu;

        [Header("Gameplay defaults")]
        [SerializeField] private CoreHealth coreHealth;
        [SerializeField] private bool applyStartingCoreHp = true;
        [SerializeField] private float startingCoreHp = 20f;
        [SerializeField] private bool ensureCoreHealthHudRuntime = true;

        private void Awake()
        {
            Application.targetFrameRate = 90;
            QualitySettings.vSyncCount = 0;

            EnsureEconomyWalletRuntime();

            if (applyStartingCoreHp && coreHealth != null)
            {
                coreHealth.SetMaxHealth(DifficultySettings.StartCoreHp);
            }

            if (audioManager != null)
            {
                audioManager.ApplyPersistedSettings();
            }

            if (settingsMenu != null)
            {
                settingsMenu.LoadValuesIntoUI();
            }
        }

        private void Start()
        {
            if (ensureCoreHealthHudRuntime &&
                FindObjectsByType<CoreHealthHud>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
            {
                var hudGo = new GameObject("CoreHealthHUD_Runtime");
                hudGo.AddComponent<CoreHealthHud>();
            }
        }

        private void EnsureEconomyWalletRuntime()
        {
            if (FindObjectsByType<EconomyWallet>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
            {
                return;
            }

            var go = new GameObject("EconomyWallet_Runtime");
            go.AddComponent<EconomyWallet>();
        }
    }
}
