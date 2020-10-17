using vNextBot.app.Dialogs;
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
    public sealed partial class KnowPage : Page
    {
        private ILoaded loaded;
        private ObservableCollection<KnowledgeBase> knowList;
        private ObservableCollection<KeyValue> types;

        public KnowPage()
        {
            types = new ObservableCollection<KeyValue>();
            knowList = new ObservableCollection<KnowledgeBase>();

            InitializeComponent();

            types.Add(new KeyValue(0, "Все"));
            using(ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Actions
                            orderby t.n_order
                            select t;

                foreach(vNextBot.Model.Action type in query)
                {
                    types.Add(new KeyValue(type.id, type.c_short_name));
                }
            }

            ActionType.ItemsSource = types;
            ActionType.SelectedIndex = 0;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            loaded = e.Parameter as ILoaded;
            loaded.OnSearchEvent += new EventHandler(OnSearch);

            updateList(null, null);
        }

        public async void updateList(int? type, string search)
        {
            if (loaded != null)
            {
                loaded.OnProgressStart("Обновление...");
            }
            knowList.Clear();
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.KnowledgeBases
                            join t1 in db.Actions on t.f_action equals t1.id
                            orderby t.dx_created descending
                            select new
                            {
                                t.id,
                                t.c_question,
                                t.jb_data,
                                t.jb_tags,
                                t.f_action,
                                t1.c_short_name,
                                t.dx_created,
                                t.b_disabled
                            };

                foreach(var item in string.IsNullOrEmpty(search) ? 
                    (type.HasValue ? query.Where(t => t.f_action == type.Value) : query): 
                    (type.HasValue ? query.Where(t => t.f_action == type.Value) : query).Where(t=>t.c_question.ToLower().Contains(search))) {
                    knowList.Add(new KnowledgeBase()
                    {
                        id = item.id,
                        c_question = item.c_question,
                        jb_data = item.jb_data,
                        jb_tags = item.jb_tags,
                        b_disabled = item.b_disabled,
                        dx_created = item.dx_created,
                        ActionName = item.c_short_name,
                        f_action = item.f_action
                    });
                }

                KnowList.ItemsSource = knowList;
            }

            if (loaded != null)
            {
                loaded.OnProgressEnd();
            }
        }

        public void OnSearch(Object sender, EventArgs e)
        {
            string searchText = (string)sender;
            if (string.IsNullOrEmpty(searchText))
            {
                updateList(null, null);
            }
            else
            {
                updateList(null, searchText.ToLower());
            }
        }

        /// <summary>
        /// Тип настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValue keyValue = (KeyValue)ActionType.SelectedItem;
            if (keyValue.Id <= 0)
            {
                updateList(null, null);
            }
            else
            {
                updateList(keyValue.Id, null);
            }
        }

        private async void AddKnowBtn_Click(object sender, RoutedEventArgs e)
        {
            KnowItemDialog dialog = new KnowItemDialog(null);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null, null);
            }
        }

        private async void KnowList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            KnowledgeBase item = (KnowledgeBase)((ListView)sender).SelectedItem;

            KnowItemDialog dialog = new KnowItemDialog(item.id);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateList(null, null);
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
                Title = "Удалить запись?",
                Content = "Удаление записи может привести к потере данных. Действительно удалить запись?",
                PrimaryButtonText = "Удалить",
                CloseButtonText = "Отменить"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && KnowList.SelectedItem is KnowledgeBase)
            {
                KnowledgeBase item = (KnowledgeBase)KnowList.SelectedItem;
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.KnowledgeBases.Remove(item);
                    db.SaveChanges();
                    updateList(null, null);
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
