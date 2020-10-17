using vNextBot.Model;
using vNextBot.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace vNextBot.app.Dialogs
{
    public sealed partial class AuthorizeDialog : ContentDialog
    {
        private ConnectionString selectConnectionString;
        private ObservableCollection<ConnectionString> hosts;

        private ConnectionStringDbName selectConnectionStringDbName;
        private ObservableCollection<ConnectionStringDbName> dbNames;
        public AuthorizeDialog()
        {
            InitializeComponent();

            VersionName.Text = "Версия: " + VersionUtil.GetAppVersion();
            hosts = new ObservableCollection<ConnectionString>();
            dbNames = new ObservableCollection<ConnectionStringDbName>();

            Host.ItemsSource = hosts;
            DbName.ItemsSource = dbNames;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            resetSummary();
            try
            {
                string host = Host.Text;
                string dbName = DbName.Text;
                string login = Login.Text;
                string password = Password.Password;
                int port = 5432;

                if(!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(dbName) && !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                {
                    if(host.IndexOf(":") > 0)
                    {
                        port = int.Parse(host.Substring(host.IndexOf(":") + 1, host.Length - (host.IndexOf(":") + 1)));
                        host = host.Substring(0, host.IndexOf(":"));
                    }
                    ApplicationContext.setConnectionString(host, port, dbName, login, password);

                    // делаем тестову проверку на соединение
                    try
                    {
                        using (ApplicationContext db = new ApplicationContext())
                        {

                        }
                    }catch(Exception e)
                    {
                        setSummary(e.Message);
                        args.Cancel = true;
                    }

                    // записываем подключения в хранилище
                    if(!await FileIO.IsFilePresent(FileIO.ACCSESS_FILE))
                        await FileIO.CreateFile(FileIO.ACCSESS_FILE);

                    string txt = await FileIO.ReadFromFile(FileIO.ACCSESS_FILE);
                    List<ConnectionString> list = FileIO.GetConnectionStrings(txt);
                    ConnectionString connectionString;
                    if(list.Any(t=>t.host == host))
                    {
                        connectionString = list.First(t => t.host == host);
                    } else
                    {
                        connectionString = new ConnectionString();
                        list.Add(connectionString);
                    }
                    connectionString.host = host;
                    connectionString.port = port.ToString();
                    connectionString.dbName = dbName;
                    connectionString.login = login;
                    connectionString.password = password;

                    txt = FileIO.ConnectionStringsToString(list);
                    await FileIO.WriteToFile(FileIO.ACCSESS_FILE, txt);
                }
                else
                {
                    setSummary("Все поля обязательны для заполнения");
                    args.Cancel = true;
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

        private async void Host_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (await FileIO.IsFilePresent(FileIO.ACCSESS_FILE))
            {
                string txt = await FileIO.ReadFromFile(FileIO.ACCSESS_FILE);
                List<ConnectionString> list = FileIO.GetConnectionStrings(txt);

                if (args.CheckCurrent())
                {
                    if (string.IsNullOrEmpty(Host.Text))
                    {
                        selectConnectionString = null;
                        hosts.Clear();
                    }
                    else
                    {
                        var results = list.Where(i => i.host.ToLower().Contains(Host.Text)).ToList();
                        hosts.Clear();

                        foreach (ConnectionString item in results)
                        {
                            hosts.Add(item);
                        }
                    }
                }
            }
        }

        private void Host_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectConnectionString = args.SelectedItem as ConnectionString;
            Host.Text = selectConnectionString.host;
            DbName.Text = selectConnectionString.dbName;
            Login.Text = selectConnectionString.login;
        }

        private async void DbName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (await FileIO.IsFilePresent(FileIO.ACCSESS_FILE))
            {
                string txt = await FileIO.ReadFromFile(FileIO.ACCSESS_FILE);
                List<ConnectionString> list = FileIO.GetConnectionStrings(txt);

                if (args.CheckCurrent())
                {
                    if (string.IsNullOrEmpty(DbName.Text))
                    {
                        selectConnectionStringDbName = null;
                        dbNames.Clear();
                    }
                    else
                    {
                        var results = list.Where(i => i.host.ToLower().Contains(Host.Text) && i.dbName.ToLower().Contains(DbName.Text)).ToList();
                        dbNames.Clear();

                        foreach (ConnectionString item in results)
                        {
                            dbNames.Add(item.ToDbNameConvert());
                        }
                    }
                }
            }
        }

        private void DbName_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectConnectionStringDbName = args.SelectedItem as ConnectionStringDbName;
            Host.Text = selectConnectionStringDbName.host;
            DbName.Text = selectConnectionStringDbName.dbName;
            Login.Text = selectConnectionStringDbName.login;
        }
    }
}
