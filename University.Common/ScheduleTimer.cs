using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.Common
{
    public static class ScheduleTimer
    {
        public static string FIRST_START = "08:00";
        public static string FIRST_END = "09:30";
        public static string SECOND_START = "09:40";
        public static string SECOND_END = "11:10";
        public static string THIRD_START = "11:30";
        public static string THIRD_END = "13:00";
        public static string FOURTH_START = "13:30";
        public static string FOURTH_END = "15:00";
        public static string FIFTH_START = "15:10";
        public static string FIFTH_END = "16:40";
        public static string SIXTH_START = "16:50";
        public static string SIXTH_END = "18:20";
        public static string SEVENTH_START = "18:30";
        public static string SEVENTH_END = "20:00";
        public static string EIGHTH_START = "20:10";
        public static string EIGHTH_END = "21:40";

        public static int GetLessonNumberFromTimeStart(string timeStart)
        {
            switch (timeStart)
            {
                case "08:00": return 1;
                case "09:40": return 2;
                case "11:30": return 3;
                case "13:30": return 4;
                case "15:10": return 5;
                case "16:50": return 6;
                case "18:30": return 7;
                case "20:10": return 8;
                default: throw new Exception("Wrong timestart string");
            }

        }

        public static (string timeStart, string timeEnd) GetLessonTimeStartEndString(int lessonNumber)
        {
            switch (lessonNumber)
            {
                case 1: return ("08:00", "09:30");
                case 2: return ("09:40", "11:10");
                case 3: return ("11:30", "13:00");
                case 4: return ("13:30", "15:00");
                case 5: return ("15:10", "16:40");
                case 6: return ("16:50", "18:20");
                case 7: return ("18:30", "20:00");
                case 8: return ("20:10", "21:40");
                default: throw new Exception("Wrong lessonNumber");
            }



        }
    }
}
