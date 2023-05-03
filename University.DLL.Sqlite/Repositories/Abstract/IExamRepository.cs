using System;
using System.Collections.Generic;
using System.Linq;
using University.DLL.Sqlite.Entities;
using System.Text;
using System.Threading.Tasks;

namespace University.DLL.Sqlite.Repositories.Abstract
{
    public interface IExamRepository
    {
        Task<List<Exam>?> GetExamsByGroupName(string groupName);
    }
}
