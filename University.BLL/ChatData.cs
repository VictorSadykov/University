﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;

namespace University.BLL
{
    public class ChatData
    {
        public long ChatId { get; set; }
        public MenuType? CurrentMenu { get; set; }
        public MenuType? NextMenu { get; set; }
        public string? SearchQueryName { get; set; }
        public string? AdminCurrentGroupEditingName { get; set; }
        public bool isEntityGroup { get; set; }
        public DayOfWeek? CurrentScheduleDay { get; set; }
        public int? CurrentWeekParity { get; set; }
        public int? DayOffset { get; set; }
    }
}
