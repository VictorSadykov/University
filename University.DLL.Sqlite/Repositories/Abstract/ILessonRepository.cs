using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;

namespace University.DLL.Sqlite.Repositories.Abstract
{
    public interface ILessonRepository
    {
        Task AddAsync(Lesson lesson);
        Task<List<Lesson>> GetAllLessonByGroupNameAsync(string groupName);
        Task<List<Lesson>> GetDayLessonsByGroupNameAsync(string groupName, int weekParity, DayOfWeek dayOfWeek);
        Task<List<Lesson>> GetWeekLessonsByGroupNameAsync(string groupName, int weekParity);
    }
}
