﻿using vNextBot.app.Dialogs;
using vNextBot.app.Model;
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
    public sealed partial class SettingPage : Page
    {
        private ILoaded loaded;
        private ObservableCollection<Setting> settingList;
        private ObservableCollection<KeyValue> types;

        public SettingPage()
        {
            types = new ObservableCollection<KeyValue>();
            settingList = new ObservableCollection<Setting>();

            InitializeComponent();

            types.Add(new KeyValue(0, "Все"));
            using(ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.SettingTypes
                            orderby t.n_order
                            select t;

                foreach(SettingTypes type in query)
                {
                    types.Add(new KeyValue(type.id, type.c_name));
                }
            }

            SettingType.ItemsSource = types;
            SettingType.SelectedIndex = 0;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            loaded = e.Parameter as ILoaded;
            loaded.OnSearchEvent += new EventHandler(OnSearch);

            updateList(null);
        }

        public async void updateList(int? type)
        {
            if (loaded != null)
            {
                loaded.OnProgressStart("Обновление...");
            }
            settingList.Clear();
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Setting
                            join t1 in db.SettingTypes on t.f_type equals t1.id
                            orderby t.c_key
                            select new
                            {
                                t.id,
                                t.c_key,
                                t.c_summary,
                                t.c_value,
                                t.f_type,
                                t1.c_name
                            };

                foreach(var item in type.HasValue ? query.Where(t => t.f_type == type.Value) : query) {
                    settingList.Add(new Setting()
                    {
                        id = item.id,
                        c_key = item.c_key,
                        c_summary = item.c_summary,
                        c_value = item.c_value,
                        f_type = item.f_type,
                        TypeName = item.c_name
                    });
                }

                SettingList.ItemsSource = settingList;
            }

            if (loaded != null)
            {
                loaded.OnProgressEnd();
            }
        }

        public void OnSearch(Object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Тип настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValue keyValue = (KeyValue)SettingType.SelectedItem;
            if (keyValue.Id <= 0)
            {
                updateList(null);
            }
            else
            {
                updateList(keyValue.Id);
            }
        }

        private async void AddSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingItemDialog dialog = new SettingItemDialog(null);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null);
            }
        }

        private async void SettingList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Setting item = (Setting)((ListView)sender).SelectedItem;

            SettingItemDialog dialog = new SettingItemDialog(item.id);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null);
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
                Content = "Удаление настройки может привести к потере данных. Действительно удалить настройку?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && SettingList.SelectedItem is Setting)
            {
                Setting item = (Setting)SettingList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Setting.Remove(item);
                    db.SaveChanges();
                    updateList(null);
                }
            }
        }

        public async void showMessageBox(String message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}
