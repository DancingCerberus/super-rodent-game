using System.Collections.Generic;
using UnityEngine;

namespace SuperRodentGame.AI
{
    public class RouteGraph : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public int Count => waypoints.Count;

        public Vector3 GetPoint(int index)
        {
            if (waypoints.Count == 0)
            {
                return transform.position;
            }

            index = Mathf.Clamp(index, 0, waypoints.Count - 1);
            return waypoints[index].position;
        }

        public int GetNextIndex(int currentIndex)
        {
            return Mathf.Min(currentIndex + 1, waypoints.Count - 1);
        }

        [ContextMenu("RebuildFromChildren")]
        public void RebuildFromChildren()
        {
            waypoints.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints.Add(transform.GetChild(i));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
