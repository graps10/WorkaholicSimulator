using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveSystem
{
    public class InventoryData
    {
        [ES3Serializable] private Dictionary<string, int> ownedItems = new();

        public event Action OnInventoryUpdated;

        public void AddItem(string id, int quantity = 1)
        {
            if (ownedItems.ContainsKey(id))
                ownedItems[id] += quantity;
            else
                ownedItems.Add(id, quantity);
            
            OnInventoryUpdated?.Invoke();
            SaveManager.SaveProgress();
            
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
            
            OnInventoryUpdated?.Invoke();
            SaveManager.SaveProgress();
            return true;
        }

        public int GetItemCount(string id) => ownedItems.ContainsKey(id) ? ownedItems[id] : 0;

        public Dictionary<string, int> GetOwnedItems() => ownedItems;
    }
}
