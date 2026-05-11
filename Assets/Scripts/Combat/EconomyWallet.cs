using System;
using UnityEngine;

namespace SuperRodentGame.Combat
{
    public class EconomyWallet : MonoBehaviour
    {
        public static EconomyWallet Instance { get; private set; }

        [SerializeField] private int startingGold = 35;

        public int Gold { get; private set; }

        public event Action<int> GoldChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Gold = Mathf.Max(0, startingGold);
            RaiseGoldChanged();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            GoldChanged = null;
        }

        public static bool TryGetInstance(out EconomyWallet wallet)
        {
            wallet = Instance;
            return wallet != null;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            RaiseGoldChanged();
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            RaiseGoldChanged();
        }

        public void Reset()
        {
            Gold = Mathf.Max(0, startingGold);
            RaiseGoldChanged();
        }

        private void RaiseGoldChanged()
        {
            GoldChanged?.Invoke(Gold);
        }
    }
}
