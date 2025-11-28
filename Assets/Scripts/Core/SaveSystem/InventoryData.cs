using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    public class InventoryData : MonoBehaviour
    {
        [ES3Serializable] private Dictionary<string, int> ownedItems = new();

        public event Action OnInventoryUpdated;

        public void AddItem(string id, int quantity = 1)
        {
            if (ownedItems.ContainsKey(id))
                ownedItems[id] += quantity;
            else
                ownedItems.Add(id, quantity);
            
            SaveManager.SaveProgress();
            OnInventoryUpdated?.Invoke();
            
            if (SaveManager.EnableSaveLoadDebugLogs)
                Debug.Log($"Added item {id}. Total: {ownedItems[id]}");
        }

        public bool TryRemoveItem(string id, int quantity = 1)
        {
            if (!ownedItems.ContainsKey(id) || ownedItems[id] < quantity)
                return false;

            ownedItems[id] -= quantity;
            if (ownedItems[id] <= 0)
                ownedItems.Remove(id);

            SaveManager.SaveProgress();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public Dictionary<string, int> GetOwnedItems() => ownedItems;
    }
}
