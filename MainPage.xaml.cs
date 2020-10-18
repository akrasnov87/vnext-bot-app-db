using vNextBot.app.Dialogs;
using vNextBot.Model;
using vNextBot.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace vNextBot.app
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page, ILoaded
    {
        public event EventHandler SearchEvent;

        public EventHandler OnSearchEvent
        {
            get
            {
                return SearchEvent;
            }
            set
            {
                SearchEvent = value;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            TitleTextBlock.Text = "База знаний";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AuthorizeDialog authorizeDialog = new AuthorizeDialog();
            ContentDialogResult result = await authorizeDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ConnectionLine.Text = ApplicationContext.getConnectionLine() + " ver. " + VersionUtil.GetAppVersion();
                TitleTextBlock.Text = "База знаний";
                myFrame.Navigate(typeof(KnowPage), this);
            }
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search.Text = "";
            Search.Visibility = Visibility.Visible;
            SearchEvent = null;
            if (secureLock.IsSelected)
            {
                TitleTextBlock.Text = "База знаний";
                myFrame.Navigate(typeof(KnowPage), this);
            } else if (config.IsSelected)
            {
                Search.Visibility = Visibility.Collapsed;
                TitleTextBlock.Text = "Настройки";
                myFrame.Navigate(typeof(SettingPage), this);
            }
            else if (accounts.IsSelected)
            {
                Search.Visibility = Visibility.Visible;
                TitleTextBlock.Text = "Аккаунты";
                myFrame.Navigate(typeof(UserPage), this);
            }
            else if (help.IsSelected)
            {
                await Launcher.LaunchUriAsync(new Uri("https://1drv.ms/w/s!AnBjlQFDvsITgf5hr8h_ZKbLYy41Ww?e=2vVqcO"));
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            mySplitView.IsPaneOpen = !mySplitView.IsPaneOpen;
        }

        public void OnProgressStart(string message)
        {
            Status.Visibility = Visibility.Visible;
            StatusName.Visibility = Visibility.Visible;
            StatusName.Text = message;
        }

        public void OnProgressEnd()
        {
            Status.Visibility = Visibility.Collapsed;
            StatusName.Visibility = Visibility.Collapsed;
            StatusName.Text = "";
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchEvent(Search.Text, EventArgs.Empty);
        }
    }
}
