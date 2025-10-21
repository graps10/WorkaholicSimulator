using TMPro;

namespace UI.Popup.Interfaces
{
    public interface IPopupWithMainText
    {
        TextMeshProUGUI MainText { get; }
        void InitializeMainText(string text);
    }
}