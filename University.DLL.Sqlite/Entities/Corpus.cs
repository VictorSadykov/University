using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class Corpus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Exam> Exams { get; set; }
    }
}
