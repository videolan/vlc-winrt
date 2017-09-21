namespace VLC_WinRT.UI.Legacy.Views
{
    interface IVLCModalFlyout
    {
        /* Return true if this flyout must be displayed in modal mode.
                * In this case, its height fits its content and it is not hidden
                * on background click. */
        bool ModalMode { get; }
    }
}
