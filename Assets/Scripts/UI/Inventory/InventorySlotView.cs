using Entities.Molds;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Button selectButton;

        private FurnitureMold _mold;
        private UnityAction<FurnitureMold> _onSelect;

        public void Initialize(FurnitureMold mold, int count, UnityAction<FurnitureMold> onSelect)
        {
            _mold = mold;
            _onSelect = onSelect;

            iconImage.sprite = mold.Icon;
            
            if (count > 1)
            {
                countText.gameObject.SetActive(true);
                countText.text = $"x{count}";
            }
            else
                countText.gameObject.SetActive(false);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onSelect?.Invoke(_mold));
        }
    }
}
