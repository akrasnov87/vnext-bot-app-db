using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.app.Model
{
    public class KeyValue
    {
        public KeyValue()
        {

        }

        public KeyValue(int id, String text)
        {
            Id = id;
            Text = text;
        }

        public int Id { get; set; }
        public String Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
