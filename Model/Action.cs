using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNextBot.Model
{
    [Table("cs_actions", Schema = "dbo")]
    public class Action
    {
        [Key]
        public int id { get; set; }

        public int n_code { get; set; }
        public string c_name { get; set; }
        public string c_short_name { get; set; }
        public string c_const { get; set; }
        public int n_order { get; set; }
        public bool b_default { get; set; }
        public bool b_disabled { get; set; }

        public override string ToString()
        {
            return c_name;
        }
    }
}
