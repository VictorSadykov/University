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
        Task Add(string groupName);
        Task AddLesson(Group group, Lesson lesson);
        Task<Group> FindByName(string groupName);
        Task<List<Group>?> GetAllGroupsByNameAsync(string groupName);
        Task ResetExamSchedule(Group group);
        Task ResetLessonSchedule(Group group);
        Task<int> WriteOrEditAsync((string groupName, string groupCode, string groupSpecialization, string groupOrientation) groupInfo);
    }
}
