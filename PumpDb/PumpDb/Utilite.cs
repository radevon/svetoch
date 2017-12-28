using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDb
{
    public class Utilite
    {
        public static string ToDateSQLite(DateTime value, bool include_time)
        {
            string format_date = "yyyy-MM-dd HH:mm:ss.fff";
            return (include_time ? value : value.Date).ToString(format_date);
        }



    }

}
