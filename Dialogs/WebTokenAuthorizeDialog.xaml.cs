using RpcSecurity.app.Utils;
using RpcSecurity.Model;
using RpcSecurity.Utils;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RpcSecurity.app.Dialogs
{
    public sealed partial class WebTokenAuthorizeDialog : ContentDialog
    {
        public WebTokenAuthorizeDialog()
        {
            InitializeComponent();
            Host.Text = Loader.getWebUrl();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            resetSummary();
            try
            {
                string host = Host.Text;
                string login = Login.Text;
                string password = Password.Password;

                Loader.CreateInstanse(host, login, password);
                if(!Loader.GetInstanse().isAuthorized())
                {
                    setSummary("Ошибка авторизации");
                    args.Cancel = true;
                } else
                {
                    using(ApplicationContext db = new ApplicationContext())
                    {
                        db.Setting.Add(new Setting() { 
                            c_key = "ALL_DEFAULT_WEB_TOKEN",
                            c_value = Loader.GetInstanse().getToken(),
                            f_type = 1,
                            c_label = "Web - токен авторизации",
                            c_summary = "",
                            sn_delete = false
                        });
                        db.SaveChanges();
                    }
                }
            } catch(Exception e) {
                setSummary(e.Message);
                args.Cancel = true;
            }
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

        private void PasswordVisible_Click(object sender, RoutedEventArgs e)
        {
            if (Password.PasswordRevealMode == PasswordRevealMode.Hidden)
            {
                Password.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                Password.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
