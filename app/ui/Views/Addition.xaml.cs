using System;
using System.Diagnostics;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Math = VLC_Wrapper.Math;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Addition : Page
    {
        public Addition()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.  The Parameter
        ///     property is typically used to configure the page.
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            int x, y;
            int.TryParse(XValue.Text, out x);
            int.TryParse(YValue.Text, out y);
            try
            {
                Math myMath = new Math();
                Result.Text = myMath.DoSomeMath(x, y).ToString();
            }
            catch (FileNotFoundException fnex)
            {
                Debug.WriteLine("Failed to intialize VLC_Wrapper");
                Debug.WriteLine("Could not find file: " + fnex.FileName);
                Debug.WriteLine(fnex.ToString());

                throw;
            }
          
        }
    }
}