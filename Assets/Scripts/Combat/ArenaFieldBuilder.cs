using SuperRodentGame.AI;
using Unity.AI.Navigation;
using UnityEngine;

namespace SuperRodentGame.Combat
{
    public class ArenaFieldBuilder : MonoBehaviour
    {
        [SerializeField] private RouteGraph routeGraph;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform corePoint;
        [SerializeField] private Transform obstacleRoot;
        [SerializeField] private Vector2 fieldSize = new Vector2(36f, 36f);
        [SerializeField] private Material pedestalMaterial;

        [ContextMenu("BuildDefaultField")]
        public void BuildDefaultField()
        {
            BuildObstacles();
            BuildElevatedTowerSockets();
            BuildWaypoints();
        }

        private void BuildObstacles()
        {
            if (obstacleRoot == null)
            {
                var root = new GameObject("ObstacleRoot");
                root.transform.SetParent(transform);
                obstacleRoot = root.transform;
            }

            for (int i = obstacleRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(obstacleRoot.GetChild(i).gameObject);
            }

            CreateObstacle(new Vector3(-4f, 0.5f, 0f), new Vector3(2f, 1f, 8f));
            CreateObstacle(new Vector3(5f, 0.5f, -4f), new Vector3(4f, 1f, 2f));
            CreateObstacle(new Vector3(7f, 0.5f, 6f), new Vector3(3f, 1f, 4f));
        }

        private void BuildElevatedTowerSockets()
        {
            Transform root = transform.Find("TowerPlatforms");
            if (root == null)
            {
                var go = new GameObject("TowerPlatforms");
                go.transform.SetParent(transform);
                root = go.transform;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(root.GetChild(i).gameObject);
            }

            CreatePedestalWithSocket(root, new Vector3(-14f, 0f, 6f), pedestalMaterial);
            CreatePedestalWithSocket(root, new Vector3(4f, 0f, -8f), pedestalMaterial);
        }

        private static void CreatePedestalWithSocket(Transform parent, Vector3 localXZ, Material mat)
        {
            float height = 2.4f;
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "TowerPedestal";
            pedestal.transform.SetParent(parent);
            pedestal.transform.localPosition = new Vector3(localXZ.x, height * 0.5f + 0.02f, localXZ.z);
            pedestal.transform.localScale = new Vector3(4.5f, height, 4.5f);
            if (mat != null)
                pedestal.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var socketGo = new GameObject("TowerSocket");
            socketGo.transform.SetParent(pedestal.transform);
            float topLocalY = height * 0.5f + 0.12f;
            socketGo.transform.localPosition = new Vector3(0f, topLocalY, 0f);

            var collider = socketGo.AddComponent<BoxCollider>();
            collider.size = new Vector3(3.2f, 0.3f, 3.2f);
            collider.center = Vector3.zero;

            socketGo.AddComponent<TowerSocket>();
        }

        private void BuildWaypoints()
        {
            if (routeGraph == null || spawnPoint == null || corePoint == null)
            {
                return;
            }

            var parent = routeGraph.transform;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(parent.GetChild(i).gameObject);
            }

            CreateWaypoint(parent, "WP_Entry", spawnPoint.position);
            CreateWaypoint(parent, "WP_LaneA", new Vector3(-8f, spawnPoint.position.y, 6f));
            CreateWaypoint(parent, "WP_LaneB", new Vector3(2f, spawnPoint.position.y, 8f));
            CreateWaypoint(parent, "WP_LaneC", new Vector3(8f, spawnPoint.position.y, 2f));
            CreateWaypoint(parent, "WP_Core", corePoint.position);
            routeGraph.RebuildFromChildren();
        }

        private void CreateObstacle(Vector3 localPos, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Obstacle";
            cube.transform.SetParent(obstacleRoot);
            cube.transform.localPosition = localPos;
            cube.transform.localScale = scale;
        }

        private static void CreateWaypoint(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(fieldSize.x, 0.2f, fieldSize.y));
        }

        /// Вызов при отсутствии сокетов в сцене: поднимает площадки и перепекает NavMesh (если есть NavMeshSurface выше по иерархии).
        public void RebuildTowerSocketsOnly(bool rebakeNavMesh = true)
        {
            BuildElevatedTowerSockets();
            if (!rebakeNavMesh)
            {
                return;
            }

            var surface = GetComponentInParent<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
            }
        }
    }
}
