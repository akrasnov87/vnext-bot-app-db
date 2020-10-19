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
    [Table("pd_users", Schema = "dbo")]
    public class User
    {
        [Key]
        public int id { get; set; }

        public string c_login { get; set; }
        public string c_fio { get; set; }
        public string c_description { get; set; }

        public bool b_disabled { get; set; }
        public string c_domain { get; set; }
        public string c_project { get; set; }
        public string c_team { get; set; }
        public int? n_pin { get; set; }

        public bool b_authorize { get; set; }

        public override string ToString()
        {
            return c_login;
        }

        public string GetDisabled()
        {
            return b_disabled ? "Отключен" : "Активен";
        }

        public string GetAuthorize()
        {
            return b_authorize ? "Авторизован" : "Не авторизован";
        }

        public string GetPin()
        {
            return n_pin.HasValue ? n_pin.Value.ToString() : "";
        }

        public SolidColorBrush getTextColor()
        {
            if(b_disabled)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            if(!b_authorize)
            {
                return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Black);
        }
    }
}
