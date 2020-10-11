using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("sd_digests", Schema = "core")]
    public class Digests
    {
        [Key]
        public int id { get; set; }
        public string c_version { get; set; }
        public string c_description { get; set; }
        public bool b_hidden { get; set; }
        public byte[] ba_file { get; set; }
    }
}
