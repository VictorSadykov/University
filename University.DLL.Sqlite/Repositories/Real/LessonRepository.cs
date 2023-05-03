using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.DLL.Sqlite.Repositories.Real
{
    public class LessonRepository : ILessonRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public async Task<List<Lesson>> GetTodayLessonsByGroupNameAsync(string groupName)
        {
            List<Lesson> allLessons = await GetAllLessonByGroupNameAsync(groupName).Result;

            return allLessons
                .Where(l => l.D)
        }

        public async Task<List<Lesson>> GetAllLessonByGroupNameAsync(string groupName)
        {
            return await _dbContext.Lessons
                .Include(l => l.Groups)
                .Where(l => l.Groups
                    .Any(g => g.Name == groupName)
                    )
                .ToListAsync();

        }
    }
}
