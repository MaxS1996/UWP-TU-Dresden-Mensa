using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC_TUD_Mensa
{
    class DateWrapper
    {
        private DateTime day;
        private CultureInfo cult;

        public DateWrapper(DateTime day, CultureInfo cult)
        {
            this.day = day;
            this.cult = cult;
        }

        public string DayOfWeek
        {
            get
            {
                return day.ToString("dddd", cult);
            }
        }

        public string Date
        {
            get
            {
                return (day.ToString("dd") + "."+day.ToString("MM"));
            }
        }

        public DateTime Day
        {
            get
            {
                return day;
            }
        }
    }
}
