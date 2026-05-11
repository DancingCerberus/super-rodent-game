using SuperRodentGame.Combat;
using UnityEngine;

namespace SuperRodentGame.Spells
{
    public class SpellCaster : MonoBehaviour
    {
        [SerializeField] private ControllerSpellInput controllerInput;
        [SerializeField] private GestureRecognizerAdapter legacyGestureRecognizer;
        [SerializeField] private AdvancedGestureRecognizer advancedGestureRecognizer;
        [SerializeField] private Transform aimingTransform;
        [SerializeField] private LayerMask mapMask;
        [SerializeField] private GameObject targetPreview;
        [SerializeField] private float castCooldown = 1.2f;
        [SerializeField] private float castRadius = 5f;
        [SerializeField] private float baseSpellDamage = 10f;
        [SerializeField] private float slowMultiplier = 0.5f;
        [SerializeField] private float slowDuration = 2.2f;
        [SerializeField] private float burnDps = 8f;
        [SerializeField] private float burnDuration = 3f;
        [SerializeField] private LayerMask enemyMask;

        private float nextCastTime;
        private Vector3 currentAimPoint;
        private bool hasAimPoint;

        private void Update()
        {
            UpdateAimPreview();
            if (Time.time < nextCastTime) return;

            SpellType spellType = SpellType.None;
            if (advancedGestureRecognizer != null && advancedGestureRecognizer.TryRecognize(out var recognized))
            {
                spellType = recognized;
            }
            else if ((controllerInput != null && controllerInput.IsPressedThisFrame())
                     || (legacyGestureRecognizer != null && legacyGestureRecognizer.IsCastGestureActive()))
            {
                spellType = SpellType.SlowSnow;
            }

            if (spellType == SpellType.None || !hasAimPoint) return;

            CastAreaSpell(spellType);
            nextCastTime = Time.time + castCooldown;
        }

        private void UpdateAimPreview()
        {
            hasAimPoint = TryResolveAimPoint(out currentAimPoint);
            if (targetPreview != null)
            {
                targetPreview.SetActive(hasAimPoint);
                if (hasAimPoint)
                {
                    targetPreview.transform.position = currentAimPoint;
                    targetPreview.transform.localScale = Vector3.one * castRadius * 2f;
                }
            }
        }

        private bool TryResolveAimPoint(out Vector3 point)
        {
            point = Vector3.zero;
            var source = aimingTransform != null ? aimingTransform : transform;
            var ray = new Ray(source.position, source.forward);
            if (Physics.Raycast(ray, out var hit, 250f, mapMask))
            {
                point = hit.point;
                return true;
            }
            return false;
        }

        private void CastAreaSpell(SpellType spellType)
        {
            var hits = Physics.OverlapSphere(currentAimPoint, castRadius, enemyMask);
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponentInParent<EnemyAgent>();
                if (enemy != null && enemy.IsAlive)
                {
                    if (spellType == SpellType.SlowSnow)
                    {
                        enemy.ApplyDamage(baseSpellDamage);
                        enemy.ApplySlow(slowMultiplier, slowDuration);
                    }
                    else if (spellType == SpellType.BurnFire)
                    {
                        enemy.ApplyDamage(baseSpellDamage * 1.5f);
                        enemy.ApplyBurn(burnDps, burnDuration);
                    }
                }
            }
        }
    }
}
