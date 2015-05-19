using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MainPages.MusicPanes;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Commands.Navigation
{
    public class ChangeMainPageMusicViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = int.Parse(parameter.ToString());
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMusic)) return;
            var frame = (App.ApplicationFrame.Content as MainPageMusic).MainPageMusicContentPresenter;
            Locator.MusicLibraryVM.MusicView = (MusicView)index;
            Switch(index, frame);
        }

        void Switch(int index, ContentPresenter frame)
        {
            switch (index)
            {
                case 0:
                    if (!(frame.Content is AlbumCollectionBase))
                        frame.Content = new AlbumCollectionBase();
                    break;
                case 1:
                    if (!(frame.Content is ArtistCollectionBase))
                        frame.Content = new ArtistCollectionBase();
                    break;
                case 2:
                    if (!(frame.Content is SongsPivotItem))
                        frame.Content = new SongsPivotItem();
                    break;
                case 3:
                    if (!(frame.Content is PlaylistPivotItem))
                        frame.Content = new PlaylistPivotItem();
                    break;
                case 4:
                    if (frame.Content is SearchMusicPane)
                    {
                        Switch((int)Locator.SettingsVM.MusicView, frame);
                        Locator.MusicLibraryVM.SearchTag = "";
                    }
                    else
                        frame.Content = new SearchMusicPane();
                    break;
            }
        }
    }
}