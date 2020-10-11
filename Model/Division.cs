using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("sd_divisions", Schema = "core")]
    public class Division
    {
        [Key]
        public int id { get; set; }

        public int? f_division { get; set; }

        public string c_name { get; set; }
        public int n_code { get; set; }
        public bool b_disabled { get; set; }

        public override string ToString()
        {
            return c_name;
        }
    }
}
