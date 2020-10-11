using Microsoft.EntityFrameworkCore.Internal;
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
    public sealed partial class AccessesPage : Page
    {
        private ILoaded loaded;
        private ObservableCollection<SimpleAccesses> accessList;
        private ObservableCollection<Accesses> overrideAccessList;
        private ObservableCollection<KeyValue> types;

        private string currentSearchText;

        public AccessesPage()
        {
            types = new ObservableCollection<KeyValue>();
            accessList = new ObservableCollection<SimpleAccesses>();
            overrideAccessList = new ObservableCollection<Accesses>();

            InitializeComponent();

            types.Add(new KeyValue(0, "Все"));
            types.Add(new KeyValue(1, "Таблица"));
            types.Add(new KeyValue(2, "Критерия"));
            types.Add(new KeyValue(3, "Функция"));
            types.Add(new KeyValue(4, "Колонки"));
            
            AccessType.ItemsSource = types;
            AccessType.SelectedIndex = 0;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            loaded = e.Parameter as ILoaded;
            loaded.OnSearchEvent += new EventHandler(OnSearch);

            updateList(0);
        }

        public void updateList(int type)
        {
            if (loaded != null)
            {
                loaded.OnProgressStart("Обновление...");
            }

            overrideAccessList.Clear();
            accessList.Clear();

            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Accesses
                            join r in db.Roles on t.f_role equals r.id into gj1
                            from x1 in gj1.DefaultIfEmpty()
                            join u in db.Users on t.f_user equals u.id into gj2
                            from x2 in gj2.DefaultIfEmpty()
                            where t.c_path == null
                            select new Accesses
                            {
                                id = t.id,
                                f_user = t.f_user,
                                UserName = x2.c_login,
                                f_role = t.f_role,
                                RoleName = x1.c_description,
                                c_name = t.c_name,
                                c_criteria = t.c_criteria,
                                c_function = t.c_function,
                                c_columns = t.c_columns,
                                b_deletable = t.b_deletable,
                                b_creatable = t.b_creatable,
                                b_editable = t.b_editable,
                                b_full_control = t.b_full_control,
                                sn_delete = t.sn_delete
                            };

                List<Accesses> accessesList;

                switch(type)
                {
                    // таблица
                    case 1:
                        accessesList = string.IsNullOrEmpty(currentSearchText) ? 
                            query.Where(t=> !string.IsNullOrEmpty(t.c_name)).ToList() : 
                            query.Where(t => !string.IsNullOrEmpty(t.c_name) && (t.c_name.IndexOf(currentSearchText) >= 0 || t.c_function.IndexOf(currentSearchText) >= 0)).ToList();
                        break;

                    // критерия
                    case 2:
                        accessesList = string.IsNullOrEmpty(currentSearchText) ? 
                            query.Where(t => !string.IsNullOrEmpty(t.c_criteria)).ToList() :
                            query.Where(t => !string.IsNullOrEmpty(t.c_criteria) && (t.c_name.IndexOf(currentSearchText) >= 0 || t.c_function.IndexOf(currentSearchText) >= 0)).ToList();
                        break;

                    // функция
                    case 3:
                        accessesList = string.IsNullOrEmpty(currentSearchText) ? 
                            query.Where(t => !string.IsNullOrEmpty(t.c_function)).ToList() :
                            query.Where(t => !string.IsNullOrEmpty(t.c_function) && (t.c_name.IndexOf(currentSearchText) >= 0 || t.c_function.IndexOf(currentSearchText) >= 0)).ToList();
                        break;

                    // колонки
                    case 4:
                        accessesList = string.IsNullOrEmpty(currentSearchText) ?
                            query.Where(t => !string.IsNullOrEmpty(t.c_columns)).ToList() :
                            query.Where(t => !string.IsNullOrEmpty(t.c_columns) && (t.c_name.IndexOf(currentSearchText) >= 0 || t.c_function.IndexOf(currentSearchText) >= 0)).ToList();
                        break;

                    default:
                        accessesList = string.IsNullOrEmpty(currentSearchText) ? 
                            query.ToList() : 
                            query.Where(t => t.c_name.IndexOf(currentSearchText) >= 0 || t.c_function.IndexOf(currentSearchText) >= 0).ToList();
                        break;
                }

                foreach(IGrouping<string, Accesses> a in from t in accessesList
                                     group t by t.getName() into g
                                     orderby g.Key
                                     select g) {
                    accessList.Add(new SimpleAccesses() { 
                        c_name = a.Key,
                        c_type = a.Max(t=>t.getTypeName()),
                        sn_delete = a.Count(t=>t.sn_delete) == a.Count()
                    });
                }

                AccessList.ItemsSource = accessList;
            }

            if (loaded != null)
            {
                loaded.OnProgressEnd();
            }
        }

        /// <summary>
        /// Событие поиска
        /// </summary>
        /// <param name="sender">Текст для поиска</param>
        /// <param name="e"></param>
        public void OnSearch(Object sender, EventArgs e)
        {
            currentSearchText = (string)sender;
            updateList(0);
        }

        /// <summary>
        /// Тип настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AccessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValue keyValue = (KeyValue)AccessType.SelectedItem;
            if (keyValue.Id <= 0)
            {
                updateList(0);
            }
            else
            {
                updateList(keyValue.Id);
            }
        }

        private async void AddAccessBtn_Click(object sender, RoutedEventArgs e)
        {
            AccessesItemDialog dialog = new AccessesItemDialog(null);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(0);
            }
        }

        private async void AccessList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Accesses item = (Accesses)((ListView)sender).SelectedItem;

            AccessesItemDialog dialog = new AccessesItemDialog(item.id);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(0);
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
                Title = "Удалить группу прав?",
                Content = "Удаление группы прав может нарушить безопасность системы. Действительно удалить группу прав?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && AccessList.SelectedItem is SimpleAccesses)
            {
                SimpleAccesses item = (SimpleAccesses)AccessList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    var query = from t in db.Accesses
                                where t.c_name == item.c_name || t.c_function == item.c_name
                                select t;

                    foreach(Accesses a in query)
                    {
                        a.sn_delete = true;
                    }

                    db.Accesses.UpdateRange(query);
                    db.SaveChanges();
                    updateList(0);
                }
            }
        }

        private async void OverrideItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Удалить права?",
                Content = "Удаление прав может нарушить безопасность системы. Действительно удалить права?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && OverrideAccessList.SelectedItem is Accesses)
            {
                Accesses item = (Accesses)OverrideAccessList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    var dbItem = db.Accesses.Find(item.id);
                    dbItem.sn_delete = true;

                    db.Accesses.Update(dbItem);

                    db.SaveChanges();

                    updateList(0);
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

        private void AccessList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            resetOverrideSummary();
            overrideAccessList.Clear();

            SimpleAccesses item = (SimpleAccesses)AccessList.SelectedItem;

            using(ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Accesses
                            join r in db.Roles on t.f_role equals r.id into gj1
                            from x1 in gj1.DefaultIfEmpty()
                            join u in db.Users on t.f_user equals u.id into gj2
                            from x2 in gj2.DefaultIfEmpty()
                            where t.c_path == null && (item.c_name == t.c_name || item.c_name == t.c_function)
                            orderby x1.n_weight descending
                            select new Accesses
                            {
                                id = t.id,
                                f_user = t.f_user,
                                UserName = x2.c_login,
                                f_role = t.f_role,
                                RoleName = x1.c_name,
                                c_name = t.c_name,
                                c_criteria = t.c_criteria,
                                c_function = t.c_function,
                                c_columns = t.c_columns,
                                b_deletable = t.b_deletable,
                                b_creatable = t.b_creatable,
                                b_editable = t.b_editable,
                                b_full_control = t.b_full_control,
                                sn_delete = t.sn_delete
                            };

                if (query.Count() == 0)
                {
                    setOverrideSummary("Переопределенных настроек не найдено.");
                } else
                {
                    foreach(Accesses i in query) {
                        overrideAccessList.Add(i);
                    }
                    
                    OverrideAccessList.ItemsSource = overrideAccessList;
                }
            }
        }
    }
}
