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
    public class SimpleAccesses
    {
        public string c_name { get; set; }
        public string c_type { get; set; }
        public bool sn_delete { get; set; }

        public SolidColorBrush getTextColor()
        {
            return sn_delete ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }
    }
}
