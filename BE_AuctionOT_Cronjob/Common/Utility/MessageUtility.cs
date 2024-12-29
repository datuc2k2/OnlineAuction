using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_AuctionOT_Cronjob.Common.Utility
{
    public static class MessageUtility
    {
        public static string ReplacePlaceholders(string? template, params object[] values)
        {
            if (template != null)
            {
                return string.Format(template, values);
            }
            return null;
        }
    }
}
