using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNextBot.app.Model
{
    public class SortItem
    {
        public SortItem() { }
        public SortItem(string property, SortDirection sort)
        {
            Property = property;
            switch(sort)
            {
                case SortDirection.DESC:
                    Direction = "desc";
                    break;

                case SortDirection.ASC:
                    Direction = "asc";
                    break;
            }
        }

        [JsonProperty("property")]
        public string Property { get; set; }
        [JsonProperty("direction")]
        public string Direction { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
