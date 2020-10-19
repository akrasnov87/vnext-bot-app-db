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
using Microsoft.EntityFrameworkCore.Internal;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace vNextBot.app.Dialogs
{
    public sealed partial class KnowItemDialog : ContentDialog
    {
        private ObservableCollection<vNextBot.Model.Action> actionTypes;
        private ObservableCollection<string> tags;

        private Guid? id;
        
        private List<string> allTags = new List<string>();

        public KnowItemDialog(Guid? id)
        {
            this.id = id;
            tags = new ObservableCollection<string>();
            actionTypes = new ObservableCollection<vNextBot.Model.Action>();

            using(ApplicationContext db = new ApplicationContext())
            {
                allTags = db.GetTags();
                foreach (string tag in allTags)
                {
                    tags.Add(tag);
                }

                foreach (vNextBot.Model.Action item in db.Actions)
                {
                    actionTypes.Add(item);
                }
            }

            InitializeComponent();

            Type.ItemsSource = actionTypes;
            Tags.ItemsSource = tags;

            if (id.HasValue)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    KnowledgeBase item = db.KnowledgeBases.Find(id);
                    Question.Text = item.c_question;
                    Tags.Text = item.GetTags();
                    Type.SelectedValue = actionTypes.First(t => t.id == item.f_action);
                    Url.Text = item.GetUrl();
                    Title.Text = item.GetTitle();
                    IsDisabled.IsChecked = item.b_disabled;
                    Date.Text = item.GetDate();
                }
            } else {
                Type.SelectedValue = actionTypes.First(t => t.c_const == "TEXT");
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

            if(string.IsNullOrEmpty(Question.Text))
            {
                setSummary("Вопрос не указан.");
                args.Cancel = true;
                return;
            }

            vNextBot.Model.Action action = (vNextBot.Model.Action)Type.SelectedValue;
            if (action.c_const == "TFS_API" ||
                action.c_const == "API" ||
                action.c_const == "DOWNLOAD" ||
                action.c_const == "LINK")
            {
                if(string.IsNullOrEmpty(Url.Text))
                {
                    setSummary("url не указан.");
                    args.Cancel = true;
                    return;
                }
            }

            if (action.c_const == "DOWNLOAD" ||
                action.c_const == "LINK" ||
                action.c_const == "TEXT")
            {
                if (string.IsNullOrEmpty(Title.Text))
                {
                    setSummary("title не указан.");
                    args.Cancel = true;
                    return;
                }
            }

            using (ApplicationContext db = new ApplicationContext())
            {
                KnowledgeBase setting;

                if (id.HasValue)
                {
                    setting = db.KnowledgeBases.Find(id);
                }
                else
                {
                    // создание
                    setting = new KnowledgeBase();
                    setting.dx_created = DateTime.Now;
                }

                setting.c_question = Question.Text;
                setting.jb_tags = Newtonsoft.Json.JsonConvert.SerializeObject(Tags.Text.Split(", "));
                setting.f_action = ((vNextBot.Model.Action)Type.SelectedValue).id;
                setting.b_disabled = IsDisabled.IsChecked.Value;

                dynamic data = new { 
                    url = Url.Text,
                    title = Title.Text
                };
                setting.jb_data = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                if (id.HasValue)
                {
                    db.KnowledgeBases.Update(setting);
                } else
                {
                    var query = from t in db.KnowledgeBases
                                where t.c_question == setting.c_question
                                select t;
                    if (query.Count() > 0)
                    {
                        setSummary("Запись с вопросом " + setting.c_question + " существует.");
                        args.Cancel = true;
                    }
                    else
                    {
                        db.KnowledgeBases.Add(setting);
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

        private void Tags_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                string[] _tags = Tags.Text.Split(", ");
                string tag = _tags[_tags.Length - 1];
                if (string.IsNullOrEmpty(tag.Trim()))
                {
                    tags.Clear();
                }
                else
                {
                    var results = allTags.Where(i => i.ToLower().Contains(tag.ToLower().Trim())).ToList();
                    tags.Clear();

                    foreach (string item in results)
                    {
                        tags.Add(item);
                    }
                }
            }
        }

        private void Tags_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string tag = args.SelectedItem as string;
            string[] _tags = Tags.Text.Split(", ");
            _tags[_tags.Length - 1] = tag;
            Tags.Text = _tags.Join(", ");
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vNextBot.Model.Action action = sender as vNextBot.Model.Action;

            if (action != null)
            {
                Title.IsEnabled = true;
                Url.IsEnabled = true;

                switch (action.c_const)
                {
                    case "TFS_API":
                    case "API":
                        Title.IsEnabled = false;
                        break;

                    case "TEXT":
                        Url.IsEnabled = false;
                        break;
                }
            }
        }
    }
}
