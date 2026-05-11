using UnityEngine;
using UnityEngine.AI;

namespace SuperRodentGame.Combat
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyVisualConfig : MonoBehaviour
    {
        [SerializeField] private GameObject ratModelPrefab;
        [SerializeField] private Vector3 modelLocalScale = Vector3.one * 0.6f;
        [SerializeField] private float agentRadius = 0.35f;
        [SerializeField] private float agentHeight = 0.9f;

        [ContextMenu("ApplyVisualAndAgentConfig")]
        public void ApplyVisualAndAgentConfig()
        {
            var agent = GetComponent<NavMeshAgent>();
            agent.radius = agentRadius;
            agent.height = agentHeight;

            if (ratModelPrefab == null)
            {
                return;
            }

            Transform modelRoot = transform.Find("ModelRoot");
            if (modelRoot == null)
            {
                var root = new GameObject("ModelRoot");
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                modelRoot = root.transform;
            }

            for (int i = modelRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(modelRoot.GetChild(i).gameObject);
            }

            var instance = Instantiate(ratModelPrefab, modelRoot);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = modelLocalScale;
        }
    }
}
