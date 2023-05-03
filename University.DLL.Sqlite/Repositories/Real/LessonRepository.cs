using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.DLL.Sqlite.Repositories.Real
{
    public class LessonRepository : ILessonRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public async Task<List<Lesson>> GetTodayLessonsByGroupNameAsync(string groupName)
        {
            List<Lesson> allGroupLessons = await Task.Run(() => GetAllLessonByGroupNameAsync(groupName).Result);

            int weekParity = WeekParityChecker.GetCurrentWeekParity();
            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

            return allGroupLessons
                .Where(l => l.WeekNumber == weekParity && l.DayNumber == dayOfWeek)
                .ToList();
        }

        public async Task<List<Lesson>> GetAllLessonByGroupNameAsync(string groupName)
        {
            return await _dbContext.Lessons
                .Include(l => l.Corpus)
                .Include(l => l.Groups)
                .Where(l => l.Groups
                    .Any(g => g.Name == groupName)
                    )
                .ToListAsync();

        }

        public async Task<List<Lesson>> GetWeekLessonsByGroupNameAsync(string groupName)
        {
            List<Lesson> allGroupLessons = await Task.Run(() => GetAllLessonByGroupNameAsync(groupName).Result);

            int weekParity = WeekParityChecker.GetCurrentWeekParity();

            return allGroupLessons.
                Where(l => l.WeekNumber == weekParity)
                .ToList();
        }
    }
}
