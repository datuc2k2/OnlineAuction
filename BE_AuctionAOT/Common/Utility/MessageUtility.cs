using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_AuctionAOT.Common.Utility
{
    public static class MessageUtility
    {
        public static string ReplacePlaceholders(string template, params object[] values)
        {
            return string.Format(template, values);
        }
    }
}
