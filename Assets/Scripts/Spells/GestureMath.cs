using System.Collections.Generic;
using UnityEngine;

namespace SuperRodentGame.Spells
{
    public static class GestureMath
    {
        public static List<Vector2> Normalize2D(IReadOnlyList<Vector3> worldSamples, int targetCount)
        {
            var projected = new List<Vector2>(worldSamples.Count);
            for (int i = 0; i < worldSamples.Count; i++)
            {
                projected.Add(new Vector2(worldSamples[i].x, worldSamples[i].z));
            }

            var resampled = Resample(projected, targetCount);
            var center = Vector2.zero;
            for (int i = 0; i < resampled.Count; i++)
            {
                center += resampled[i];
            }
            center /= resampled.Count;

            float maxDist = 0.0001f;
            for (int i = 0; i < resampled.Count; i++)
            {
                resampled[i] -= center;
                maxDist = Mathf.Max(maxDist, resampled[i].magnitude);
            }

            for (int i = 0; i < resampled.Count; i++)
            {
                resampled[i] /= maxDist;
            }

            return resampled;
        }

        public static float MeanDistance(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b)
        {
            int count = Mathf.Min(a.Count, b.Count);
            if (count == 0) return float.MaxValue;

            float total = 0f;
            for (int i = 0; i < count; i++)
            {
                total += Vector2.Distance(a[i], b[i]);
            }

            return total / count;
        }

        private static List<Vector2> Resample(List<Vector2> points, int targetCount)
        {
            var result = new List<Vector2>(targetCount);
            if (points.Count == 0)
            {
                return result;
            }

            if (points.Count == 1)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    result.Add(points[0]);
                }
                return result;
            }

            float[] cumulative = new float[points.Count];
            cumulative[0] = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                cumulative[i] = cumulative[i - 1] + Vector2.Distance(points[i - 1], points[i]);
            }

            float totalLength = cumulative[cumulative.Length - 1];
            if (totalLength <= 0.0001f)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    result.Add(points[0]);
                }
                return result;
            }

            for (int i = 0; i < targetCount; i++)
            {
                float t = (i / (float)(targetCount - 1)) * totalLength;
                int seg = 1;
                while (seg < cumulative.Length && cumulative[seg] < t) seg++;
                seg = Mathf.Clamp(seg, 1, points.Count - 1);

                float segStartDist = cumulative[seg - 1];
                float segEndDist = cumulative[seg];
                float segT = Mathf.Approximately(segStartDist, segEndDist) ? 0f : (t - segStartDist) / (segEndDist - segStartDist);
                result.Add(Vector2.Lerp(points[seg - 1], points[seg], segT));
            }

            return result;
        }
    }
}
