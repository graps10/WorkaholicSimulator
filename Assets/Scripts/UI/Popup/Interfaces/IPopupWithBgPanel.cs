using UnityEngine.UI;

namespace UI.Popup.Interfaces
{
    public interface IPopupWithBgPanel
    {
        Image PanelBg { get; }
        void InitializeBgPanel();
    }
}