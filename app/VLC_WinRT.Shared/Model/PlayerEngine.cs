namespace VLC_WinRT.Model
{
    public enum PlayerEngine
    {
        VLC,
        MediaFoundation, // Should never be used anymore since we have D3D11 output and hardware acceleration working, but we keep it in case we want to debug without working VLC libs
        BackgroundMFPlayer
    }
}
