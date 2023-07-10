using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;
using University.MiniMethods;

namespace University.DLL.Sqlite.Repositories.Real
{
    public class ExamRepository : IExamRepository
    {
        private readonly UniversityDbContext _dbContext;

        public ExamRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Exam exam)
        {
            var entry = _dbContext.Entry(exam);
            if (entry.State == EntityState.Detached)
            {
                await _dbContext.Exams
                    .AddAsync(exam);

            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Exam>?> GetExamsByGroupNameAsync(string groupName)
        {
            return await _dbContext.Exams
                .Include(e => e.Group)
                .Include(e => e.Teacher)
                .Where(e => e.Group.Name == groupName)
                .ToListAsync();
        }

        public async Task<List<Exam>?> GetExamsByTeacherFullNameAsync(string teacherFullName)
        {
            (string fistName, string lastName, string secondName) = NameAnalyser.FullNameParseToStrings(teacherFullName);

            return await _dbContext.Exams
                .Include(e => e.Group)
                .Include(e => e.Teacher)
                .Where(e => 
                    e.Teacher.FirstName == fistName &&
                    e.Teacher.LastName == lastName &&
                    e.Teacher.SecondName == secondName)
                .ToListAsync();
        }
    }
}
