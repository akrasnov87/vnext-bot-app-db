using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcSecurity.app.Model
{
    public class FilterItem
    {
        public FilterItem() { }
        public FilterItem(string property, object value)
        {
            Property = property;
            Value = value;
            Operator = "=";
        }

        public FilterItem(string property, String @operator, object value)
        {
            Property = property;
            Value = value;
            Operator = @operator;
        }

        [JsonProperty("property")]
        public string Property { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
