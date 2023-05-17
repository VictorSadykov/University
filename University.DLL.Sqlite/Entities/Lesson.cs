﻿using System;
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
        public LessonType LessonType { get; set; } // 0 - Лекция, 1 - Практика, 2 - лаб. р
        public int TimeNumber { get; set; } // Какая пара по счёту 
        public DayOfWeek DayNumber { get; set; }
        public int WeekNumber { get; set; }
        public string TeacherFullName { get; set; }
        public string CabNumber { get; set; }
        public string CorpusLetter { get; set; }
        public string SubGroup { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

    }

    public enum LessonType
    {
        Lecture,
        Practice,
        LabWork
    }
}
