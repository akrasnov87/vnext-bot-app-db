using vNextBot.app.Model;
using vNextBot.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace vNextBot.app.Dialogs
{
    public sealed partial class SettingItemDialog : ContentDialog
    {
        private ObservableCollection<SettingTypes> settingTypes;

        private int? id;

        public SettingItemDialog(int? id)
        {
            this.id = id;

            settingTypes = new ObservableCollection<SettingTypes>();

            using(ApplicationContext db = new ApplicationContext())
            {
                foreach(SettingTypes item in db.SettingTypes)
                {
                    settingTypes.Add(item);
                }
            }

            InitializeComponent();

            Type.ItemsSource = settingTypes;

            if (id.HasValue)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Setting item = db.Setting.Find(id);
                    Key.Text = item.c_key;
                    Value.Text = item.c_value;
                    Type.SelectedValue = settingTypes.First(t => t.id == item.f_type);
                    Label.Text = item.c_summary;
                }
            } else {
                Type.SelectedValue = settingTypes.First(t => t.c_const == "TEXT");
            }
        }

        /// <summary>
        /// Сохранение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            resetSummary();
            using (ApplicationContext db = new ApplicationContext())
            {
                Setting setting;

                if (id.HasValue)
                {
                    setting = db.Setting.Find(id);
                }
                else
                {
                    // создание
                    setting = new Setting();
                }

                setting.c_key = Key.Text;
                setting.c_value = Value.Text;
                setting.f_type = ((SettingTypes)Type.SelectedValue).id;
                setting.c_summary = Label.Text;

                if (id.HasValue)
                {
                    db.Setting.Update(setting);
                } else
                {
                    var query = from t in db.Setting
                                where t.c_key == setting.c_key
                                select t;
                    if (query.Count() > 0)
                    {
                        setSummary("Настройка " + setting.c_key + " существует.");
                        args.Cancel = true;
                    }
                    else
                    {
                        db.Setting.Add(setting);
                    }
                }
                db.SaveChanges();
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

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
    }
}
