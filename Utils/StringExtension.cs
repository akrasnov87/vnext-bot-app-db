using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vNextBot.Utils
{
    public static class StringExtension
    {
        public static string ToEmpty(this string input)
        {
            return input == null ? "" : input;
        }
    }
}
