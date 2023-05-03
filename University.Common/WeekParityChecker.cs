using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.Common
{
    public static class WeekParityChecker
    {
        public static int GetCurrentWeekParity()
        {
            return DateTime.Now.DayOfYear / 7 % 2 + 1;
        }
    }
}
