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
        Task AddAsync(Group group);
        //Task AddLessonAsync(Group group, Lesson lesson);
        Task<Group> FindByNameAsync(string groupName);
        Task ResetExamScheduleAsync(Group group);
        Task ResetLessonScheduleAsync(Group group);
        Task<int> WriteOrEditAsync((string groupName, string groupCode, string groupSpecialization, string groupOrientation) groupInfo);
    }
}
