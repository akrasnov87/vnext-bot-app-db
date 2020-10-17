using vNextBot.app.Dialogs;
using vNextBot.app.Model;
using vNextBot.app.Utils;
using vNextBot.Model;
using vNextBot.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace vNextBot.app
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserPage : Page
    {
        private ILoaded loaded;
        private ObservableCollection<User> userList;

        public UserPage()
        {
            userList = new ObservableCollection<User>();

            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            loaded = e.Parameter as ILoaded;
            loaded.OnSearchEvent += new EventHandler(OnSearch);

            updateList();
        }

        public async void updateList()
        {
            if (loaded != null)
            {
                loaded.OnProgressStart("Обновление...");
            }
            userList.Clear();
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from u in db.Users
                            orderby u.c_login
                            select u;

                foreach(var item in query) {
                    userList.Add(item);
                }

                UserList.ItemsSource = userList;
            }

            if (loaded != null)
            {
                loaded.OnProgressEnd();
            }
        }

        public void OnSearch(Object sender, EventArgs e)
        {
            
        }

        private async void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            UserItemDialog dialog = new UserItemDialog(null);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList();
            }
        }

        private async void UserList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            User item = (User)((ListView)sender).SelectedItem;

            UserItemDialog dialog = new UserItemDialog(item.id);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList();
            }
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Удалить настройку?",
                Content = "Удаление аккаунта может привести к потере данных. Действительно удалить аккаунт?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && UserList.SelectedItem is User)
            {
                User item = (User)UserList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Users.Remove(item);
                    db.SaveChanges();
                    updateList();
                }
            }
        }

        public async void showMessageBox(String message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        private async void ItemReset_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Сбросить информацию?",
                Content = "Потребуется повторная авторизация. Действительно сбросить аккаунт?",
                PrimaryButtonText = "Сбросить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && UserList.SelectedItem is User)
            {
                User item = (User)UserList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.UserReset(item.id);
                    updateList();
                }
            }
        }
    }
}
