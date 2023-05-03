﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDateTime { get; set; }
        public string TeacherFullName { get; set; }
        public ExaminationType ExaminationType { get; set; }
        public string CabNumber { get; set; }
        public int CorpusId { get; set; }
        public Corpus Corpus { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }

    public enum ExaminationType
    {
        Exam
    }
}
