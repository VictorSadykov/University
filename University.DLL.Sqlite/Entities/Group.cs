using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SubGroupNumber { get; set; }
        public DateTime? PracticeDateStart { get; set; }
        public DateTime? PracticeDateEnd { get; set; }
        public string? PracticeTeacherFullName { get; set; }
        public List<Lesson>? Lessons { get; set; }
        public List<Exam>? Exams { get; set; }
    }
}
