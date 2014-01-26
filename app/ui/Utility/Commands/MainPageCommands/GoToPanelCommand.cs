using VLC_WINRT.Common;

namespace VLC_WINRT.Utility.Commands.MainPage
{
    public class GoToPanelCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var frame = App.ApplicationFrame;
            var page = frame.Content as Views.MainPage;
            if (page != null)
            {
                page.ChangedSectionsHeadersState(int.Parse(parameter.ToString()));
            }
        }
    }
}