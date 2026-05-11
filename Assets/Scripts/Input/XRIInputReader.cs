using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperRodentGame.Input
{
    public class XRIInputReader : MonoBehaviour
    {
        public static XRIInputReader Instance;

        [Header("Actions")]
        public InputActionReference spellCastAction;
        public InputActionReference grabAction;
        public InputActionReference uiPauseAction;
        public InputActionReference drawHoldAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            spellCastAction?.action.Enable();
            grabAction?.action.Enable();
            uiPauseAction?.action.Enable();
            drawHoldAction?.action.Enable();
        }

        private void OnDisable()
        {
            spellCastAction?.action.Disable();
            grabAction?.action.Disable();
            uiPauseAction?.action.Disable();
            drawHoldAction?.action.Disable();
        }

        public bool SpellCastPressed()
        {
            return spellCastAction != null &&
                   spellCastAction.action.WasPressedThisFrame();
        }

        public bool GrabHeld()
        {
            return grabAction != null &&
                   grabAction.action.IsPressed();
        }

        public bool PausePressed()
        {
            return uiPauseAction != null &&
                   uiPauseAction.action.WasPressedThisFrame();
        }

        public bool DrawHeld()
        {
            return drawHoldAction != null &&
                   drawHoldAction.action.IsPressed();
        }
    }
}