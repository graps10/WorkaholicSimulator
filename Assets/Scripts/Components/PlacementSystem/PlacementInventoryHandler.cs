using Core.SaveSystem;
using Entities.Molds;
using UnityEngine;

namespace Components.PlacementSystem
{
    public class PlacementInventoryHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnPlacementSuccess += HandleItemPlaced;
                PlacementManager.Instance.OnFurnitureRemoved += HandleItemRemoved;
            }
        }

        private void OnDisable()
        {
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.OnPlacementSuccess -= HandleItemPlaced;
                PlacementManager.Instance.OnFurnitureRemoved -= HandleItemRemoved;
            }
        }
        
        private void HandleItemPlaced(FurnitureMold mold)
        {
            SaveManager.Progress.Inventory.TryRemoveItem(mold.ID);
            //Debug.Log($"[Inventory] Removed 1 {mold.name} (Placed).");
        }

        private void HandleItemRemoved(FurnitureMold mold)
        {
            SaveManager.Progress.Inventory.AddItem(mold.ID);
            //Debug.Log($"[Inventory] Added 1 {mold.name} (Removed from socket).");
        }
    }
}