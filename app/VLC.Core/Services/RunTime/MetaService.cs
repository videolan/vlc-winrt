using VLC.Model.Library;

namespace VLC.Services.RunTime
{
    public abstract class MetaService
    {
        protected readonly MediaLibrary MediaLibrary;
        protected readonly NetworkListenerService NetworkListenerService;

        protected MetaService(MediaLibrary mediaLibrary, NetworkListenerService networkListenerService)
        {
            MediaLibrary = mediaLibrary;
            NetworkListenerService = networkListenerService;
        }
    }
}