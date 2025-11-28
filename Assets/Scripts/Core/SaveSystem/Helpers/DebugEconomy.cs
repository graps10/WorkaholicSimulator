using UnityEngine;

namespace Core.SaveSystem.Helpers
{
    public class DebugEconomy: MonoBehaviour
    {
        [ContextMenu("Add 10000")] public void Add10000() => SaveManager.Progress.Wallet.AddMoney(10000);
        [ContextMenu("Add 1000")] public void Add1000() => SaveManager.Progress.Wallet.AddMoney(1000);
        [ContextMenu("Add 100")] public void Add100() => SaveManager.Progress.Wallet.AddMoney(100);
        
        [ContextMenu("Try Spend 100")] public void TrySpend100() => SaveManager.Progress.Wallet.TrySpendMoney(100);
        [ContextMenu("Try Spend 1000")] public void TrySpend1000() => SaveManager.Progress.Wallet.TrySpendMoney(1000);
        [ContextMenu("Try Spend 10000")] public void TrySpend10000() => SaveManager.Progress.Wallet.TrySpendMoney(10000);
        
        [ContextMenu("Check Console for Balance")]
        public void CheckBalance()
        {
            Debug.Log($"Balance: {SaveManager.Progress.Wallet.CurrentMoney}");
        }
    }
}