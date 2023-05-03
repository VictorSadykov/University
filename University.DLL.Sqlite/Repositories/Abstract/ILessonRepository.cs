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
        Task<List<Lesson>> GetAllLessonByGroupNameAsync(string groupName);
        Task<List<Lesson>> GetTodayLessonsByGroupNameAsync(string groupName);
        Task<List<Lesson>> GetWeekLessonsByGroupNameAsync(string groupName, int weekParity);
    }
}
