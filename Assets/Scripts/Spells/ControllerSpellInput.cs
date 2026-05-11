using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperRodentGame.Spells
{
    public class ControllerSpellInput : MonoBehaviour
    {
        public bool IsPressedThisFrame()
        {
            var mouse = Mouse.current;

            if (mouse != null)
            {
                return mouse.leftButton.wasPressedThisFrame;
            }

            return false;
        }
    }
}