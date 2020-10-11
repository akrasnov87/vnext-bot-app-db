using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.Model
{
    public class ConnectionStringDbName : ConnectionString
    {
        public override string ToString()
        {
            return dbName;
        }
    }
}
