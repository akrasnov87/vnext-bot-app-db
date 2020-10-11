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
    [Table("cd_settings", Schema = "core")]
    public class Setting
    {
        [Key]
        public int id { get; set; }

        public string c_key { get; set; }
        public string c_value { get; set; }
        public int f_type { get; set; }
        public string c_label { get; set; }
        public string c_summary { get; set; }
        public int? f_division { get; set; }
        public int? f_user { get; set; }
        public int? f_role { get; set; }
        public bool sn_delete { get; set; }

        public string TypeName;
        public string DivisionName;
        public string RoleName;
        public string UserName;

        public SolidColorBrush getTextColor()
        {
            return sn_delete ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }
    }
}
