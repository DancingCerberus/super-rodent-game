using UnityEngine;
using SuperRodentGame.Input;

namespace SuperRodentGame.Spells
{
    public class GestureRecognizerAdapter : MonoBehaviour, IGestureRecognizer
    {
        [SerializeField] private bool debugAlwaysCast;

        public bool IsCastGestureActive()
        {
            if (debugAlwaysCast)
            {
                return true;
            }

            return XRIInputReader.Instance.SpellCastPressed();
        }
    }
}
