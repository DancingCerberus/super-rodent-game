using System.Collections.Generic;
using UnityEngine;
using SuperRodentGame.Input;

namespace SuperRodentGame.Spells
{
    public class AdvancedGestureRecognizer : MonoBehaviour
    {
        [SerializeField] private Transform samplingTransform;
        [SerializeField] private LayerMask mapMask;
        [SerializeField] private float sampleInterval = 0.02f;
        [SerializeField] private float minDrawDuration = 0.25f;
        [SerializeField] private int normalizedPointCount = 48;
        [SerializeField] private float acceptanceDistance = 0.42f;


        private readonly List<Vector3> samples = new List<Vector3>();
        private float sampleTimer;
        private float drawTimer;
        private bool isDrawing;

        public bool TryRecognize(out SpellType spellType)
        {
            spellType = SpellType.None;

            bool hold = IsHoldActive();
            if (hold)
            {
                if (!isDrawing)
                {
                    samples.Clear();
                    sampleTimer = 0f;
                    drawTimer = 0f;
                    isDrawing = true;
                }

                drawTimer += Time.deltaTime;
                sampleTimer += Time.deltaTime;
                if (sampleTimer >= sampleInterval)
                {
                    sampleTimer = 0f;
                    if (TrySamplePoint(out var point))
                    {
                        samples.Add(point);
                    }
                }

                return false;
            }

            if (!isDrawing)
            {
                return false;
            }

            isDrawing = false;
            if (drawTimer < minDrawDuration || samples.Count < 8)
            {
                samples.Clear();
                return false;
            }

            var normalized = GestureMath.Normalize2D(samples, normalizedPointCount);
            samples.Clear();

            float circleScore = GestureMath.MeanDistance(normalized, BuildCircleTemplate(normalizedPointCount));
            float starScore = GestureMath.MeanDistance(normalized, BuildStarTemplate(normalizedPointCount));

            if (circleScore > acceptanceDistance && starScore > acceptanceDistance)
            {
                return false;
            }

            spellType = circleScore <= starScore ? SpellType.SlowSnow : SpellType.BurnFire;
            return true;
        }

        private bool TrySamplePoint(out Vector3 point)
        {
            point = Vector3.zero;
            var source = samplingTransform != null ? samplingTransform : transform;
            var ray = new Ray(source.position, source.forward);
            if (Physics.Raycast(ray, out var hit, 200f, mapMask))
            {
                point = hit.point;
                return true;
            }

            return false;
        }

        private bool IsHoldActive()
        {
           if (XRIInputReader.Instance != null)
            {
               return XRIInputReader.Instance.GrabHeld();
            }

    return false;
}

        private static List<Vector2> BuildCircleTemplate(int points)
        {
            var result = new List<Vector2>(points);
            for (int i = 0; i < points; i++)
            {
                float t = (i / (float)(points - 1)) * Mathf.PI * 2f;
                result.Add(new Vector2(Mathf.Cos(t), Mathf.Sin(t)));
            }
            return result;
        }

        private static List<Vector2> BuildStarTemplate(int points)
        {
            // 5-point star polyline template
            Vector2[] control =
            {
                new Vector2(0f, 1f),
                new Vector2(0.22f, 0.22f),
                new Vector2(0.95f, 0.22f),
                new Vector2(0.35f, -0.14f),
                new Vector2(0.58f, -0.9f),
                new Vector2(0f, -0.42f),
                new Vector2(-0.58f, -0.9f),
                new Vector2(-0.35f, -0.14f),
                new Vector2(-0.95f, 0.22f),
                new Vector2(-0.22f, 0.22f),
                new Vector2(0f, 1f)
            };

            var line = new List<Vector3>(control.Length);
            for (int i = 0; i < control.Length; i++)
            {
                line.Add(new Vector3(control[i].x, 0f, control[i].y));
            }

            return GestureMath.Normalize2D(line, points);
        }
    }
}
