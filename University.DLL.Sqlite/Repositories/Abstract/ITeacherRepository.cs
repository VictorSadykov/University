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
        Teacher? FindByFullName(string fullName);
        Task AddAsync(Teacher teacher);
        List<Teacher> FindAllByLastName(string lastName);
        Task ResetLessonScheduleAsync(Teacher teacher);
        Task<Teacher> FindByIdAsync(int id);
        List<(int, DayOfWeek)> GetWorkingDays(string fullName);
    }
}
