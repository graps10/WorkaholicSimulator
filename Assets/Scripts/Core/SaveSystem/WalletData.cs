using System;
using UnityEngine;

namespace Core.SaveSystem
{
    public class WalletData
    {
        public static event Action<int> OnMoneyChanged;

        [ES3Serializable] private int currentMoney;

        public int CurrentMoney => currentMoney;
        
        public WalletData() => currentMoney = 0; 

        public void AddMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Use TrySpendMoney to subtract money.");
                return;
            }

            currentMoney += amount;
            NotifyAndSave();
        }

        public bool TrySpendMoney(int amount)
        {
            if (amount < 0) return false;

            if (HasEnough(amount))
            {
                currentMoney -= amount;
                NotifyAndSave();
                return true;
            }

            if (SaveManager.EnableSaveLoadDebugLogs) 
                Debug.Log($"Not enough money. Current: {currentMoney}, Needed: {amount}");
            
            return false;
        }

        public bool HasEnough(int amount) => currentMoney >= amount;

        private void NotifyAndSave()
        {
            OnMoneyChanged?.Invoke(currentMoney);
            SaveManager.SaveProgress();
            
            if (SaveManager.EnableSaveLoadDebugLogs) 
                Debug.Log($"Wallet updated: {currentMoney}");
        }
    }
}
