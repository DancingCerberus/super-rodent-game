using UnityEngine;

namespace SuperRodentGame.Combat
{
    /// Большой trigger-бокс вокруг арены.
    /// Когда противник выходит за пределы (выброшен игроком) — засчитывается как убийство и начисляется золото.
    [AddComponentMenu("TD/Arena Kill Zone")]
    public class ArenaKillZone : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            var enemy = other.GetComponentInParent<EnemyAgent>();
            if (enemy != null && enemy.IsAlive)
                enemy.ApplyDamage(float.MaxValue);
        }
    }
}
