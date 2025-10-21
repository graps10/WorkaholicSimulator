namespace UI.Popup.Interfaces
{
    public interface IPopupWithCloseButton
    {
        CustomButtonController CloseButton { get; }
        void InitializeCloseButton();
    }
}