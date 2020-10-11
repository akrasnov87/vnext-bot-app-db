using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("sv_objects", Schema = "core")]
    public class Objects
    {
        [Key]
        public string table_name { get; set; }
        public string table_type { get; set; }
        public string table_title { get; set; }
        public string primary_key { get; set; }
        public string table_comment { get; set; }
        public string table_schema { get; set; }

        public override string ToString()
        {
            return table_name;
        }
    }
}
