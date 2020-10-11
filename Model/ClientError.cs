using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    [Table("sd_client_errors", Schema = "core")]
    public class ClientError
    {
        [Key]
        public Guid id { get; set; }
        public string c_message { get; set; }
        public string c_code { get; set; }
        public DateTime d_created { get; set; }
        public int? fn_user { get; set; }
        public string c_version { get; set; }
        public string c_platform { get; set; }
    }
}
