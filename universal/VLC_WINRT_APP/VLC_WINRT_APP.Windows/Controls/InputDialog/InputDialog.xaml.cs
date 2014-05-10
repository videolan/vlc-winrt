/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Common;
using Path = Windows.UI.Xaml.Shapes.Path;

namespace VLC_WINRT.Views.Controls.InputDialog
{
    public sealed partial class InputDialog : UserControl
    {
        private AlwaysExecutableCommand _command;
        public InputDialog()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Collapsed;
        }

        public void Show(string logoPath, string title, string subTitle, string actionButton, AlwaysExecutableCommand actionCommand)
        {
            var b = new Binding
            {
                Source = logoPath
            };
            BindingOperations.SetBinding(Logo, Path.DataProperty, b);
            Title.Text = title;
            Subtitle.Text = subTitle;
            ActionButton.Content = actionButton;
            _command = actionCommand;
            this.Visibility = Visibility.Visible;
        }
        private void CancelDialog(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ActionButtonClicked(object sender, RoutedEventArgs e)
        {
            _command.Execute(TextBox.Text);
            this.Visibility = Visibility.Collapsed;
        }
    }
}
