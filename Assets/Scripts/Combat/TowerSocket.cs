using UnityEngine;

namespace SuperRodentGame.Combat
{
    /// Слот на возвышенности: одна башня на сокет. Raycast и UI — через TowerPlacementController.
    public class TowerSocket : MonoBehaviour
    {
        [SerializeField] private Transform placementAnchor;

        private GameObject occupant;

        public bool IsOccupied => occupant != null;

        public Transform PlacementAnchor =>
            placementAnchor != null ? placementAnchor : transform;

        public bool TryPlaceTower(GameObject towerPrefab)
        {
            if (towerPrefab == null || IsOccupied)
            {
                return false;
            }

            occupant = Instantiate(towerPrefab, PlacementAnchor.position, PlacementAnchor.rotation, PlacementAnchor);
            return true;
        }

        public bool TryDestroyOccupant()
        {
            if (occupant == null)
            {
                return false;
            }

            Destroy(occupant);
            occupant = null;
            return true;
        }
    }
}
