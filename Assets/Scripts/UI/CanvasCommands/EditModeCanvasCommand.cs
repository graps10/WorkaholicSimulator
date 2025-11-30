using UI.FurnitureShop;
using UI.Inventory;
using UnityEngine;

namespace UI.CanvasCommands
{
    public class EditModeCanvasCommand : CanvasCommand
    {
        public const string Path = "ScriptableObjects/ObjectPool/UI/CanvasCommands/EditModeCanvasCommandPoolInfo";
        public override string CanvasCommandPath => Path;

        [Header("Buttons")] 
        [SerializeField] private CustomButtonController openFurnitureShopButton;
        [SerializeField] private CustomButtonController openInventoryButton;

        public override void Initialize(CanvasReceivers.CanvasReceiver receiver)
        {
            base.Initialize(receiver);
            AddListeners();
        }

        private void AddListeners()
        {
            if (openFurnitureShopButton != null)
                openFurnitureShopButton.onClick.AddListener(OpenFurnitureShop);

            if (openInventoryButton != null)
                openInventoryButton.onClick.AddListener(OpenInventory);
        }

        private void RemoveListeners()
        {
            if (openFurnitureShopButton != null)
                openFurnitureShopButton.onClick.RemoveAllListeners();

            if (openInventoryButton != null)
                openInventoryButton.onClick.RemoveAllListeners();
        }

        private void OpenFurnitureShop() => FurnitureShopPopup.Create();

        private void OpenInventory() => InventoryPopup.Create();

        public override void Dispose()
        {
            RemoveListeners();
            base.Dispose();
        }
    }
}