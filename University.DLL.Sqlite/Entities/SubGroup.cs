using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class SubGroup
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public List<Lesson>? Lessons { get; set; }
    }
}
