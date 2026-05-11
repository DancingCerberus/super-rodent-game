using UnityEngine;

namespace SuperRodentGame.Content
{
    public class BlenderModelAnchor : MonoBehaviour
    {
        [SerializeField] private Transform modelRoot;
        [SerializeField] private bool lockScale = true;
        [SerializeField] private Vector3 targetScale = Vector3.one;

        private void OnValidate()
        {
            if (modelRoot == null || !lockScale) return;
            modelRoot.localScale = targetScale;
        }
    }
}
