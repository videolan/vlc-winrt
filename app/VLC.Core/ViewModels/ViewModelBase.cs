using VLC.Utils;

namespace VLC.ViewModels
{
    public abstract class ViewModelBase : BindableBase
    {
        public virtual void Initialize() { }
        public virtual void Stop() { }
    }
}