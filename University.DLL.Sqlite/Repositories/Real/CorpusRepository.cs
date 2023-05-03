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
    public class CorpusRepository : ICorpusRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public async Task<List<Corpus>?> GetAllAsync()
        {
            return await _dbContext.Corpuses
                .ToListAsync();
        }
    }
}
