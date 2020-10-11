using RpcSecurity.app.Dialogs;
using RpcSecurity.app.Model;
using RpcSecurity.app.Utils;
using RpcSecurity.Model;
using RpcSecurity.Utils;
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

namespace RpcSecurity.app
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private ILoaded loaded;
        private ObservableCollection<Setting> settingList;
        private ObservableCollection<Setting> overrideSettingList;
        private ObservableCollection<KeyValue> types;

        public SettingPage()
        {
            types = new ObservableCollection<KeyValue>();
            settingList = new ObservableCollection<Setting>();
            overrideSettingList = new ObservableCollection<Setting>();

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
            overrideSettingList.Clear();
            settingList.Clear();
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Setting
                            join t1 in db.SettingTypes on t.f_type equals t1.id
                            where t.f_division == null && t.f_user == null && t.f_role == null
                            orderby t.c_key
                            select new
                            {
                                t.id,
                                t.c_key,
                                t.c_label,
                                t.c_summary,
                                t.c_value,
                                t.f_division,
                                t.f_role,
                                t.f_type,
                                t.f_user,
                                t1.c_name
                            };

                foreach(var item in type.HasValue ? query.Where(t => t.f_type == type.Value) : query) {
                    settingList.Add(new Setting()
                    {
                        id = item.id,
                        c_key = item.c_key,
                        c_label = item.c_label,
                        c_summary = item.c_summary,
                        c_value = item.c_value,
                        f_division = item.f_division,
                        f_role = item.f_role,
                        f_type = item.f_type,
                        f_user = item.f_user,
                        TypeName = item.c_name
                    });
                }

                if(!settingList.Any(t=>t.c_key == "ALL_URL"))
                {
                    AddWebTokenBtn.Visibility = Visibility.Collapsed;
                    AddUrlBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    AddUrlBtn.Visibility = Visibility.Collapsed;
                    AddWebTokenBtn.Visibility = Visibility.Visible;
                }

                if(settingList.Any(t=>t.c_key == "ALL_BIRTH_DAY"))
                {
                    AddBirthDayBtn.Visibility = Visibility.Collapsed;
                } else
                {
                    AddBirthDayBtn.Visibility = Visibility.Visible;
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

        private async void OverrideItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Удалить настройку?",
                Content = "Удаление настройки может привести к потере данных. Действительно удалить настройку?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && OverrideSettingList.SelectedItem is Setting)
            {
                Setting item = (Setting)OverrideSettingList.SelectedItem;
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

        private void setOverrideSummary(string message)
        {
            if (OverrideSummary != null)
            {
                OverrideSummary.Visibility = Visibility.Visible;
                OverrideSummary.Text = message;
            }
        }

        private void resetOverrideSummary()
        {
            if (OverrideSummary != null)
            {
                OverrideSummary.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOverrideSummary();
            overrideSettingList.Clear();
            Setting item = (Setting)SettingList.SelectedItem;

            using(ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Setting
                            join d in db.Divisions on t.f_division equals d.id into gj
                            from x in gj.DefaultIfEmpty()
                            join r in db.Roles on t.f_role equals r.id into gj1
                            from x1 in gj1.DefaultIfEmpty()
                            join u in db.Users on t.f_user equals u.id into gj2
                            from x2 in gj2.DefaultIfEmpty()
                            where t.c_key == item.c_key && t.id != item.id
                            select new
                            {
                                t.id,
                                t.f_division,
                                t.f_role,
                                t.f_user,
                                t.c_value,
                                t.c_key,
                                c_division = x.c_name,
                                c_role = x1.c_description,
                                c_user = x2.c_login,
                                sn_delete = t.sn_delete
                            };

                if(query.Count() == 0)
                {
                    setOverrideSummary("Переопределенных настроек не найдено.");
                } else
                {
                    foreach(var i in query)
                    {
                        overrideSettingList.Add(new Setting() {
                            id = i.id,
                            c_key = i.c_key,
                            c_value = i.c_value,
                            f_division = i.f_division,
                            f_role = i.f_role,
                            f_user = i.f_user,
                            DivisionName = i.c_division,
                            RoleName = i.c_role,
                            UserName = i.c_user,
                            sn_delete = i.sn_delete
                        });
                    }

                    OverrideSettingList.ItemsSource = overrideSettingList;
                }
            }
        }

        private async void AddWebTokenBtn_Click(object sender, RoutedEventArgs e)
        {
            WebTokenAuthorizeDialog dialog = new WebTokenAuthorizeDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null);
            }
        }

        private async void AddUrlBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingItemDialog dialog = new SettingItemDialog("ALL_URL", 1, "Основной адрес сайта");
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null);
            }
        }

        private async void AddBirthDayBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingItemDialog dialog = new SettingItemDialog("ALL_BIRTH_DAY", 1, "Дата рождения решения - YYYY-MM-DD");
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null);
            }
        }
    }
}
