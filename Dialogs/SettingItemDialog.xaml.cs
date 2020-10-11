using RpcSecurity.app.Model;
using RpcSecurity.app.Utils;
using RpcSecurity.Model;
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

namespace RpcSecurity.app.Dialogs
{
    public sealed partial class SettingItemDialog : ContentDialog
    {
        private ObservableCollection<SettingTypes> settingTypes;
        private ObservableCollection<Division> divisions;
        private ObservableCollection<User> users;
        private ObservableCollection<Role> roles;

        private int? id;

        private List<Division> allDivisions = new List<Division>();
        private List<User> allUsers = new List<User>();
        private List<Role> allRoles = new List<Role>();

        private Division selectDivision;
        private User selectUser;
        private Role selectRole;

        public SettingItemDialog(string key, int type, string label): 
            this(null)
        {
            Key.Text = key;
            Type.SelectedValue = settingTypes.First(t => t.id == type);
            Label.Text = label;

        }

        public SettingItemDialog(int? id)
        {
            this.id = id;

            settingTypes = new ObservableCollection<SettingTypes>();
            divisions = new ObservableCollection<Division>();
            users = new ObservableCollection<User>();
            roles = new ObservableCollection<Role>();

            using(ApplicationContext db = new ApplicationContext())
            {
                foreach(SettingTypes item in db.SettingTypes)
                {
                    settingTypes.Add(item);
                }

                allDivisions = db.Divisions.ToList();
                foreach (Division item in allDivisions)
                {
                    divisions.Add(item);
                }

                allUsers = db.Users.ToList();
                foreach (User item in allUsers)
                {
                    users.Add(item);
                }

                allRoles = db.Roles.ToList();
                foreach (Role item in allRoles)
                {
                    roles.Add(item);
                }
            }

            InitializeComponent();

            Type.ItemsSource = settingTypes;
            DivisionType.ItemsSource = divisions;
            UserType.ItemsSource = users;
            RoleType.ItemsSource = roles;

            if (id.HasValue)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Setting item = db.Setting.Find(id);
                    Key.Text = item.c_key;
                    Value.Text = item.c_value;
                    Type.SelectedValue = settingTypes.First(t => t.id == item.f_type);
                    Label.Text = item.c_label;

                    if (item.f_division.HasValue)
                    {
                        divisions.Clear();
                        selectDivision = allDivisions.First(t => t.id == item.f_division);
                        DivisionType.Text = selectDivision.c_name;
                    }
                    else
                    {
                        selectDivision = null;
                        DivisionType.Text = "";
                    }

                    if (item.f_role.HasValue)
                    {
                        roles.Clear();
                        selectRole = allRoles.First(t => t.id == item.f_role);
                        RoleType.Text = selectRole.c_name;
                    }
                    else
                    {
                        selectRole = null;
                        RoleType.Text = "";
                    }

                    if (item.f_user.HasValue)
                    {
                        users.Clear();
                        selectUser = allUsers.First(t => t.id == item.f_user);
                        UserType.Text = selectUser.c_login;
                    }
                    else
                    {
                        selectUser = null;
                        UserType.Text = "";
                    }
                }
            } else
            {
                selectDivision = null;
                DivisionType.Text = "";

                selectUser = null;
                UserType.Text = "";

                selectRole = null;
                RoleType.Text = "";

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
                setting.c_label = Label.Text;
                setting.c_summary = "";

                if (selectDivision != null)
                {
                    setting.f_division = selectDivision.id;
                }
                else
                {
                    setting.f_division = null;
                }

                if (selectUser != null)
                {
                    setting.f_user = selectUser.id;
                }
                else
                {
                    setting.f_user = null;
                }

                if (selectRole != null)
                {
                    setting.f_role = selectRole.id;
                }
                else
                {
                    setting.f_role = null;
                }

                /*if (DivisionType.SelectedValue != null && ((Division)DivisionType.SelectedValue).id != int.MinValue)
                    setting.f_division = ((Division)DivisionType.SelectedValue).id;

                if (UserType.SelectedValue != null && ((User)UserType.SelectedValue).id != int.MinValue)
                    setting.f_user = ((User)UserType.SelectedValue).id;

                if (RoleType.SelectedValue != null && ((Role)RoleType.SelectedValue).id != int.MinValue)
                    setting.f_role = ((Role)RoleType.SelectedValue).id;*/

                if (id.HasValue)
                {
                    db.Setting.Update(setting);
                } else
                {
                    var query = from t in db.Setting
                                where t.c_key == setting.c_key && t.f_division == setting.f_division && t.f_role == setting.f_role && t.f_user == setting.f_user
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

        private void DivisionType_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(DivisionType.Text))
                {
                    selectDivision = null;
                    divisions.Clear();
                }
                else
                {
                    var results = allDivisions.Where(i => i.c_name.ToLower().Contains(DivisionType.Text)).ToList();
                    divisions.Clear();

                    foreach (Division item in results)
                    {
                        divisions.Add(item);
                    }
                }
            }
        }

        private void DivisionType_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectDivision = args.SelectedItem as Division;
        }

        private void UserType_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(UserType.Text))
                {
                    selectUser = null;
                    users.Clear();
                }
                else
                {
                    var results = allUsers.Where(i => i.c_login.ToLower().Contains(UserType.Text)).ToList();
                    users.Clear();

                    foreach (User item in results)
                    {
                        users.Add(item);
                    }
                }
            }
        }

        private void UserType_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectUser = args.SelectedItem as User;
        }

        private void RoleType_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(RoleType.Text))
                {
                    selectRole = null;
                    roles.Clear();
                }
                else
                {
                    var results = allRoles.Where(i => i.c_name.ToLower().Contains(RoleType.Text)).ToList();
                    roles.Clear();

                    foreach (Role item in results)
                    {
                        roles.Add(item);
                    }
                }
            }
        }

        private void RoleType_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectRole = args.SelectedItem as Role;
        }
    }
}
