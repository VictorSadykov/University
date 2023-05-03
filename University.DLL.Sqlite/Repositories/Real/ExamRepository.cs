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
    public class ExamRepository : IExamRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public async Task<List<Exam>?> GetExamsByGroupName(string groupName)
        {
            return await _dbContext.Exams
                .Include(e => e.Group)
                .Include(e => e.Corpus)
                .Where(e => e.Group.Name == groupName)
                .ToListAsync();
        }
    }
}
