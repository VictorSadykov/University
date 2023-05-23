using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SecondName { get; set; }
        public List<Lesson>? Lessons { get; set; } = new List<Lesson>();
        public List<Exam>? Exams { get; set; } = new List<Exam>();
    }
}
