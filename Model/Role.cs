using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("pd_roles", Schema = "core")]
    public class Role
    {
        [Key]
        public int id { get; set; }

        public string c_name { get; set; }
        public string c_description { get; set; }
        public int n_weight { get; set; }
        public bool sn_delete { get; set; }

        public override string ToString()
        {
            return c_name;
        }
    }
}
