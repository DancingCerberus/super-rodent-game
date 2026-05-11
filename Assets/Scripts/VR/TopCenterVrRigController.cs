using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperRodentGame.VR
{
    public class TopCenterVrRigController : MonoBehaviour
    {
        [SerializeField] private Transform rigRoot;
        [SerializeField] private Vector3 centerOffset = new Vector3(0f, 18f, 0f);
        [SerializeField] private float yawSpeedDegrees = 60f;

        private void Start()
        {
            if (rigRoot == null)
            {
                rigRoot = transform;
            }

            rigRoot.position += centerOffset;
        }

        private void Update()
        {
            float yawInput = 0f;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.qKey.isPressed) yawInput -= 1f;
                if (keyboard.eKey.isPressed) yawInput += 1f;
            }

            if (Mathf.Abs(yawInput) > 0.001f)
            {
                rigRoot.Rotate(Vector3.up, yawInput * yawSpeedDegrees * Time.deltaTime, Space.World);
            }
        }
    }
}