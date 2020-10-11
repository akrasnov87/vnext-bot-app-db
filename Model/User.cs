using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("pd_users", Schema = "core")]
    public class User
    {
        [Key]
        public int id { get; set; }

        public string c_login { get; set; }

        public override string ToString()
        {
            return c_login;
        }
    }
}
