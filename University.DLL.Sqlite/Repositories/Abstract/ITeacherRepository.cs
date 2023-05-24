using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;

namespace University.DLL.Sqlite.Repositories.Abstract
{
    public interface ITeacherRepository
    {
        Task<Teacher?> FindByFullNameAsync(string fullName);
        Task AddAsync(Teacher teacher);
        Task<List<Teacher>> FindAllByLastName(string lastName);
        Task ResetLessonScheduleAsync(Teacher teacher);
        Task<Teacher> FindByIdAsync(int id);
    }
}
