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
    public class GroupRepository : IGroupRepository
    {
        private readonly UniversityDbContext _dbContext = new UniversityDbContext();

        public async Task<List<Group>?> GetAllGroupsByNameAsync(string groupName)
        {
            return await _dbContext.Groups
                .Where(g => g.Name == groupName)
                .ToListAsync();
        }
    }
}
