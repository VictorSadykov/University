using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.Configuration
{
    public static class WeekParityChecker
    {
        public static bool CheckCurrentWeek()
        {
            return DateTime.Now.DayOfYear / 7 % 2 == 0;
        }
    }
}
