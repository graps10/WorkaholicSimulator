using Entities.Molds;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.FurnitureShop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        private FurnitureMold _mold;
        private UnityAction<FurnitureMold> _onBuyClick;

        public void Initialize(FurnitureMold mold, UnityAction<FurnitureMold> onBuyClick)
        {
            _mold = mold;
            _onBuyClick = onBuyClick;

            iconImage.sprite = mold.Icon;
            priceText.text = $"${mold.Price}";

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClicked);
        }

        private void OnClicked() => _onBuyClick?.Invoke(_mold);
    }
}
