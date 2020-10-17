using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace vNextBot.Model
{
    [Table("cd_knowledge_base", Schema = "dbo")]
    public class KnowledgeBase
    {
        [Key]
        public Guid id { get; set; }

        public string c_question { get; set; }

        public int? f_action { get; set; }

        public string ActionName;

        [Column(TypeName = "jsonb")]
        public string jb_data { get; set; }

        [Column(TypeName = "jsonb")]
        public string jb_tags { get; set; }

        public DateTime dx_created { get; set; }

        public bool b_disabled { get; set; }

        public string GetTags()
        {
            if(!string.IsNullOrEmpty(jb_tags))
            {
                string[] tags = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(jb_tags);
                return tags.Join(", ");
            }
            return "";
        }

        public string GetDate()
        {
            return dx_created.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public string GetDisabled()
        {
            return b_disabled ? "Отключен" : "Активен";
        }

        public string GetUrl()
        {
            if (!string.IsNullOrEmpty(jb_data))
            {
                dynamic tags = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jb_data);
                return tags.url;
            }
            return "";
        }

        public string GetTitle()
        {
            if (!string.IsNullOrEmpty(jb_data))
            {
                dynamic tags = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jb_data);
                return tags.title;
            }
            return "";
        }

        public SolidColorBrush getTextColor()
        {
            return b_disabled ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }
    }
}
