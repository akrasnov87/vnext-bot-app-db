using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    public class ConnectionString
    {
        public string host { get; set; }
        public string port { get; set; }
        public string dbName { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public override string ToString()
        {
            return host + ":" + port;
        }

        public ConnectionStringDbName ToDbNameConvert()
        {
            ConnectionStringDbName item = new ConnectionStringDbName();
            item.dbName = dbName;
            item.host = host;
            item.login = login;
            item.password = password;
            item.port = port;

            return item;
        }
    }
}
