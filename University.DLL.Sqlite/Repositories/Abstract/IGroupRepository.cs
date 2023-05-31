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
        Group FindByName(string groupName);
        List<(int, DayOfWeek)> GetWorkingDays(string groupName);
        Task ResetExamScheduleAsync(Group group);
        Task ResetLessonScheduleAsync(Group group);
        Task UpdateCodeAsync(Group group, string code);
        Task UpdateOrientationAsync(Group group, string orientation);
        Task UpdatePracticeEndDateAsync(Group group, string startDate);
        Task UpdatePracticeStartDateAsync(Group group, string fullName);
        Task UpdatePracticeTeacherFullNameAsync(Group group, string fullName);
        Task UpdateSpecializationAsync(Group group, string specialization);
        Task<int> WriteOrEditAsync((string groupName, string groupCode, string groupSpecialization, string groupOrientation) groupInfo);
    }
}
