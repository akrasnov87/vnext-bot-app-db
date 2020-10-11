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
    public sealed partial class AccessesItemDialog : ContentDialog
    {
        private int? id;

        private ObservableCollection<User> users;
        private ObservableCollection<Role> roles;
        private ObservableCollection<Objects> objects;
        private ObservableCollection<String> functions;

        private List<User> allUsers = new List<User>();
        private List<Role> allRoles = new List<Role>();
        private List<Objects> allObjects = new List<Objects>();
        private List<String> allFuncions = new List<string>();

        private User selectUser;
        private Role selectRole;
        private Objects selectObject;
        private string selectFunction;
        private bool IsOpened = false;

        public AccessesItemDialog(int? id)
        {
            this.id = id;

            users = new ObservableCollection<User>();
            roles = new ObservableCollection<Role>();
            objects = new ObservableCollection<Objects>();
            functions = new ObservableCollection<string>();

            using(ApplicationContext db = new ApplicationContext())
            {
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

                allObjects = db.Objects.ToList();
                foreach (Objects item in allObjects)
                {
                    objects.Add(item);
                }
            }

            allFuncions = Loader.getRemoteObjects();
            foreach (String item in allFuncions)
            {
                functions.Add(item);
            }

            InitializeComponent();

            UserItems.ItemsSource = users;
            RoleItems.ItemsSource = roles;
            Name.ItemsSource = objects;
            Function.ItemsSource = functions;

            if (id.HasValue)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Accesses item = db.Accesses.Find(id);
                    if (!string.IsNullOrEmpty(item.c_name))
                    {
                        objects.Clear();
                        selectObject = allObjects.First(t => t.table_name == item.c_name);
                        Name.Text = selectObject.table_name;
                    }
                    else
                    {
                        selectObject = null;
                        Name.Text = "";
                    }

                    //Name.Text = item.c_name == null ? "" : item.c_name;
                    Criteria.Text = item.c_criteria == null ? "" : item.c_criteria;
                    Function.Text = item.c_function == null ? "" : item.c_function;

                    if (!string.IsNullOrEmpty(item.c_function))
                    {
                        functions.Clear();
                        selectFunction = allFuncions.FirstOrDefault(t => t == item.c_function);
                        Function.Text = selectFunction == null ? item.c_function : selectFunction;
                    }
                    else
                    {
                        selectFunction = null;
                        Function.Text = "";
                    }

                    Columns.Text = item.c_columns == null ? "" : item.c_columns;

                    IsDeletable.IsChecked = item.b_deletable;
                    IsCreatable.IsChecked = item.b_creatable;
                    IsEditable.IsChecked = item.b_editable;
                    IsFullControl.IsChecked = item.b_full_control;
                    Recovery.IsChecked = item.sn_delete;
                    Recovery.Visibility = item.sn_delete ? Visibility.Visible : Visibility.Collapsed;

                    if (item.f_role.HasValue)
                    {
                        roles.Clear();
                        selectRole = allRoles.First(t => t.id == item.f_role);
                        RoleItems.Text = selectRole.c_name;
                    } else
                    {
                        selectRole = null;
                        RoleItems.Text = "";
                    }

                    if (item.f_user.HasValue)
                    {
                        users.Clear();
                        selectUser = allUsers.First(t => t.id == item.f_user);
                        UserItems.Text = selectUser.c_login;
                    }
                    else
                    {
                        selectUser = null;
                        UserItems.Text = "";
                    }
                }
            } else
            {
                Recovery.Visibility = Visibility.Collapsed;
                selectUser = null;
                UserItems.Text = "";
                selectRole = null;
                RoleItems.Text = "";
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
                Accesses accesses;

                if (id.HasValue)
                {
                    accesses = db.Accesses.Find(id);
                }
                else
                {
                    // создание
                    accesses = new Accesses();
                }

                accesses.c_name = Name.Text;
                accesses.c_criteria = Criteria.Text;
                accesses.c_function = Function.Text;
                accesses.c_columns = Columns.Text;

                accesses.b_deletable = IsDeletable.IsChecked.Value;
                accesses.b_creatable = IsCreatable.IsChecked.Value;
                accesses.b_editable = IsEditable.IsChecked.Value;
                accesses.b_full_control = IsFullControl.IsChecked.Value;
                accesses.sn_delete = Recovery.IsChecked.Value;

                if(selectUser != null)
                {
                    accesses.f_user = selectUser.id;
                } else
                {
                    accesses.f_user = null;
                }

                if (selectRole != null)
                {
                    accesses.f_role = selectRole.id;
                }
                else
                {
                    accesses.f_role = null;
                }

                if (id.HasValue)
                {
                    db.Accesses.Update(accesses);
                } else
                {
                    var query = from t in db.Accesses
                                where t.f_role == accesses.f_role && t.f_user == accesses.f_user && t.c_name == accesses.c_name && t.c_criteria == accesses.c_criteria && t.c_function == accesses.c_function && t.c_columns == accesses.c_columns
                                select t;
                    if (query.Count() > 0)
                    {
                        setSummary("Безопасность существует.");
                        args.Cancel = true;
                    }
                    else
                    {
                        db.Accesses.Add(accesses);
                    }
                }
                db.SaveChanges();

                if(Loader.updateAccessCache() != "SUCCESS")
                {
                    setSummary("Ошибка обновления кэша безопасности. Информация не применена на сервере.");
                    args.Cancel = true;
                }
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

        private void UserItems_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(UserItems.Text))
                {
                    selectUser = null;
                    users.Clear();
                }
                else
                {
                    var results = allUsers.Where(i => i.c_login.ToLower().Contains(UserItems.Text)).ToList();
                    users.Clear();

                    foreach (User item in results)
                    {
                        users.Add(item);
                    }
                }
            }
        }

        private void UserItems_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectUser = args.SelectedItem as User;
        }

        private void RoleItems_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent() && IsOpened)
            {
                if (string.IsNullOrEmpty(RoleItems.Text))
                {
                    selectRole = null;
                    roles.Clear();
                }
                else
                {
                    var results = allRoles.Where(i => i.c_name.ToLower().Contains(RoleItems.Text)).ToList();
                    roles.Clear();

                    foreach (Role item in results)
                    {
                        roles.Add(item);
                    }
                }
            }

            IsOpened = true;
        }

        private void RoleItems_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectRole = args.SelectedItem as Role;
        }

        private void Name_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(Name.Text))
                {
                    selectObject = null;
                    objects.Clear();
                }
                else
                {
                    var results = allObjects.Where(i => i.table_name.ToLower().Contains(Name.Text)).ToList();
                    objects.Clear();

                    foreach (Objects item in results)
                    {
                        objects.Add(item);
                    }
                }
            }
        }

        private void Name_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectObject = args.SelectedItem as Objects;
        }

        private void Function_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                if (string.IsNullOrEmpty(Function.Text))
                {
                    selectFunction = null;
                    functions.Clear();
                }
                else
                {
                    var results = allFuncions.Where(i => i.ToLower().Contains(Function.Text)).ToList();
                    functions.Clear();

                    foreach (string item in results)
                    {
                        functions.Add(item);
                    }
                }
            }
        }

        private void Function_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            selectFunction = args.SelectedItem as string;
        }
    }
}
