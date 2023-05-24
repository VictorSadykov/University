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
    public class TeacherRepository : ITeacherRepository
    {
        private readonly UniversityDbContext _dbContext;

        public TeacherRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Teacher teacher)
        {
            var entry = _dbContext.Entry(teacher);
            if (entry.State == EntityState.Detached)
            {
                await _dbContext.Teachers
                    .AddAsync(teacher);

            }

            
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetLessonScheduleAsync(Teacher teacher)
        {
            Teacher? foundTeacher = await _dbContext.Teachers
                 .Include(t => t.Lessons)
                 .Where(t => t.FirstName == teacher.FirstName &&
                             t.LastName == teacher.LastName &&
                             t.SecondName == teacher.SecondName)
                 .FirstOrDefaultAsync();

            foundTeacher.Lessons = new List<Lesson>();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Teacher> FindByIdAsync(int id)
        {
            return await _dbContext.Teachers.Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Teacher?> FindByFullNameAsync(string fullName)
        {
            string[] strings = fullName
                .Replace(".", "")
                .Split(" ");

            string firstName = strings[1];
            string lastName = strings[0];
            string secondName = strings[2];

            return await _dbContext.Teachers
                .Where(t => t.FirstName == firstName &&
                            t.LastName == lastName &&
                            t.SecondName == secondName)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Teacher>> FindAllByLastName(string lastName)
        {
            return await _dbContext.Teachers
                .Where(t => t.LastName == lastName)
                .ToListAsync();
        }
    }
}
