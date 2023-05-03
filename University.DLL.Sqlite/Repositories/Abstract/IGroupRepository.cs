using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;

namespace University.DLL.Sqlite.Repositories.Abstract
{
    public interface IGroupRepository
    {
        Task<Group?> GetGroupByNameAsync(string groupName); 
    }
}
