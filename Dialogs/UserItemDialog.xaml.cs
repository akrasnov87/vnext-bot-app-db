using vNextBot.app.Model;
using vNextBot.app.Utils;
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
using vNextBot.Utils;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace vNextBot.app.Dialogs
{
    public sealed partial class UserItemDialog : ContentDialog
    {
        private int? id;

        public UserItemDialog(int? id)
        {
            this.id = id;
            InitializeComponent();

            if (id.HasValue)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    User item = db.Users.Find(id);
                    Login.Text = item.c_login.ToEmpty();
                    FIO.Text = item.c_fio.ToEmpty();
                    
                    Description.Text = item.c_description.ToEmpty();
                    IsDisabled.IsChecked = item.b_disabled;
                    IsAuthorized.IsChecked = item.b_authorize;
                }
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
                User user;

                if (id.HasValue)
                {
                    user = db.Users.Find(id);
                }
                else
                {
                    // создание
                    user = new User();
                    user.c_domain = "Compulink";
                    user.b_disabled = true;
                }

                user.c_fio = FIO.Text;
                user.c_description = Description.Text;

                user.b_disabled = IsDisabled.IsChecked.Value;
                user.b_authorize = IsAuthorized.IsChecked.Value;

                if (id.HasValue)
                {
                    db.Users.Update(user);
                } else
                {
                    var query = from t in db.Users
                                where t.c_login == user.c_login
                                select t;
                    if (query.Count() > 0)
                    {
                        setSummary("Аккаунт " + user.c_login + " существует.");
                        args.Cancel = true;
                    }
                    else
                    {
                        db.Users.Add(user);
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
