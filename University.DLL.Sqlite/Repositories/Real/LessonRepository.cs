using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.Common;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;
using University.MiniMethods;

namespace University.DLL.Sqlite.Repositories.Real
{
    public class LessonRepository : ILessonRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public LessonRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Lesson>> GetDayLessonsByGroupNameAsync(string groupName, int weekParity, DayOfWeek dayOfWeek)
        {
            return await _dbContext.Lessons
                .Include(l => l.Groups)
                .Where(l => l.Groups
                    .Any(g => g.Name == groupName) &&
                    l.WeekNumber == weekParity &&
                    l.DayNumber == dayOfWeek)
                .ToListAsync();
                
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

        public async Task<List<Lesson>> GetWeekLessonsByGroupNameAsync(string groupName, int weekParity)
        {
            return await _dbContext.Lessons
                .Include(l => l.Groups)
                .Where(l => l.Groups
                    .Any(g => g.Name == groupName) &&
                    l.WeekNumber == weekParity)
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetWeekLessonsByTeacherFullNameAsync(string teacherFullName, int weekParity)
        {
            (string firstName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(teacherFullName);

            return await _dbContext.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Groups)
                .Where(l => l.Teacher.FirstName == firstName &&
                            l.Teacher.SecondName == secondName &&
                            l.Teacher.LastName == lastName &&
                            l.WeekNumber == weekParity)
                .ToListAsync();
        }

        public async Task AddAsync(Lesson lesson)
        {
            var entry = _dbContext.Entry(lesson);
            if (entry.State == EntityState.Detached)
            {
                await _dbContext.Lessons.AddAsync(lesson);  

            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
