using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class LessonData
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public int DayOfWeekNumber { get; set; }
        public int TimeNumber { get; set; }
        public string BuildingName { get; set; }
        public string CabName { get; set; }
    }
}
