using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LessonType { get; set; } // 0 - Лекция, 1 - Практика, 2 - лаб. р
        public List<LessonData> LessonDatas { get; set; }
    }
}
