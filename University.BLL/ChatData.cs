﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.BLL
{
    public class ChatData
    {
        public long ChatId { get; set; }
        public MenuType CurrentMenu { get; set; }
        public string? GroupName { get; set; }
    }
}
