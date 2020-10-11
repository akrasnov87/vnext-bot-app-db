using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace RpcSecurity.Model
{
    [Table("pd_accesses", Schema = "core")]
    public class Accesses
    {
        
        [Key]
        public int id { get; set; }
        public int? f_user { get; set; }
        public string UserName;
        public int? f_role { get; set; }
        public string RoleName;
        public string c_name { get; set; }
        public string c_criteria { get; set; }
        public string c_function { get; set; }
        public string c_columns { get; set; }
        public string c_path { get; set; }
        public bool b_deletable { get; set; }
        public bool b_creatable { get; set; }
        public bool b_editable { get; set; }
        public bool b_full_control { get; set; }
        public bool sn_delete { get; set; }
        
        public string getName()
        {
            return String.IsNullOrEmpty(c_function) ? c_name : c_function;
        }

        public string getTypeName()
        {
            return string.IsNullOrEmpty(c_function) ? "Таблица" : "Функция";
        }

        public string getDeletable()
        {
            return b_deletable ? "Да" : "Нет";
        }

        public string getCreatable()
        {
            return b_creatable ? "Да" : "Нет";
        }

        public string getEditable()
        {
            return b_editable ? "Да" : "Нет";
        }

        public string getFullControl()
        {
            return b_full_control ? "Да" : "Нет";
        }

        public SolidColorBrush getTextColor()
        {
            return sn_delete ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }
    }
}
