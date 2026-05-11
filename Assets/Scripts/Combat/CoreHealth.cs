using System;
using UnityEngine;
using UnityEngine.Events;

namespace SuperRodentGame.Combat
{
    public class CoreHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private UnityEvent onCoreDestroyed;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;

        public event Action<float, float> HealthChanged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            RaiseHealthChanged();
        }

        public void SetMaxHealth(float value)
        {
            maxHealth = Mathf.Max(1f, value);
            CurrentHealth = maxHealth;
            RaiseHealthChanged();
        }

        public event Action CoreDestroyed;

        public void ApplyDamage(float damage)
        {
            if (CurrentHealth <= 0f) return;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            RaiseHealthChanged();
            if (CurrentHealth <= 0f)
            {
                onCoreDestroyed?.Invoke();
                CoreDestroyed?.Invoke();
            }
        }

        public void Restore()
        {
            CurrentHealth = maxHealth;
            RaiseHealthChanged();
        }

        private void RaiseHealthChanged()
        {
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void OnDestroy()
        {
            HealthChanged = null;
        }
    }
}
