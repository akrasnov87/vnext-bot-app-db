using RpcSecurity.app.Utils;
using RpcSecurity.Utils;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RpcSecurity.app
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UpdatePage : Page
    {
        public UpdatePage()
        {
            InitializeComponent();

            string version = VersionUtil.GetLastVersion();

            VersionTitle.Text = version;
            CheckList.Text = VersionUtil.GetDescriptionVersion(version);

            string[] data = version.Split(".");
            string status = "";
            switch(int.Parse(data[2]))
            {
                case 0:
                    status = "альфа версия";
                    break;

                case 1:
                    status = "бета версия";
                    break;

                case 2:
                    status = "выпуск-кандидат";
                    break;

                case 3:
                    status = "публичный выпуск";
                    break;
            }

            string birthDay = Loader.getBirthDay();
            if(birthDay != null)
            {
                DateTime dt = DateTime.Parse(birthDay);
                dt = dt.AddDays(Double.Parse(data[1]));
                dt = dt.AddMinutes(Double.Parse(data[3]));
                Description.Text = string.Format("Версия приложения {0}.{1} от {2} - {3}.", data[0], data[1], dt.ToString("dd.MM.yyyy HH:mm:ss"), status);
            } else
            {
                Description.Text = "Перетащите файл APK на изображение";
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        /// <summary>
        /// загрузка файла для обновления APK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Download_Drop(object sender, DragEventArgs e)
        {
            resetSummary();
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    try
                    {
                        var storageFile = items[0] as StorageFile;
                        await VersionUtil.FileUpload(storageFile);
                        string version = VersionUtil.GetLastVersion();

                        showMessageBox("Версия обновлена: " + version);

                        VersionTitle.Text = "Текущая версия: " + version;

                    } catch(Exception exc)
                    {
                        setSummary(exc.Message);
                        VersionTitle.Text = "0.0.0.0";
                    }
                }
            }
        }

        public async void showMessageBox(String message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }

        private void setSummary(string message)
        {
            if (Summary != null)
            {
                Summary.Visibility = Visibility.Visible;
                Summary.Text = message;
            }
        }

        private void resetSummary()
        {
            if (Summary != null)
            {
                Summary.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckListUpdate_Click(object sender, RoutedEventArgs e)
        {
            string version = VersionUtil.GetLastVersion();
            VersionUtil.UpdateDescriptionVersion(version, CheckList.Text);
        }
    }
}
