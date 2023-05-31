using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University.DLL.Sqlite.Entities;
using University.DLL.Sqlite.Repositories.Abstract;

namespace University.DLL.Sqlite.Repositories.Real
{
    public class GroupRepository : IGroupRepository
    {
        private readonly UniversityDbContext _dbContext;

        public GroupRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Group group)
        {
            var entry = _dbContext.Entry(group);
            if (entry.State == EntityState.Detached)
            {
                await _dbContext.Groups
                    .AddAsync(group);

            }

            await _dbContext.SaveChangesAsync();
        }

        /*public async Task AddLessonAsync(Group group, Lesson lesson)
        {
            Group foundGroup = await _dbContext.Groups
                 .Include(g => g.Lessons)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Lessons.Add(lesson);

            await _dbContext.SaveChangesAsync();
        }*/

        public List<(int, DayOfWeek)> GetWorkingDays(string groupName)
        {
            Group? foundGroup = FindByName(groupName);

            List<(int WeekNumber, DayOfWeek DayNumber)> weekNumberAndDays = new List<(int, DayOfWeek)>();

            

            foreach (var item in foundGroup.Lessons)
            {
                (int weekNumber, DayOfWeek dayOfWeek) dayInfo = (item.WeekNumber, item.DayNumber);
                if (!weekNumberAndDays.Contains(dayInfo))
                {
                    weekNumberAndDays.Add(dayInfo);
                }
            }

            List<(int, DayOfWeek)> sortedLessons = weekNumberAndDays
                .OrderBy(l => l.WeekNumber)
                .ThenBy(l => l.DayNumber)
                .ToList();

            return sortedLessons;
        }

        public async Task ResetLessonScheduleAsync(Group group)
        {
            Group? foundGroup = await _dbContext.Groups
                 .Include(g => g.Lessons)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Lessons = new List<Lesson>();
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetExamScheduleAsync(Group group)
        {
            Group? foundGroup = await _dbContext.Groups
                 .Include(g => g.Exams)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Exams = new List<Exam>();
        }

        public Group? FindByName(string groupName)
        {
            string groupNameLower = groupName.ToLower();

            foreach (var item in _dbContext.Groups.Include(g => g.Lessons).ThenInclude(l => l.Teacher).Include(g => g.Exams))
            {
                if (item.Name.ToLower() == groupNameLower)
                {
                    return item;
                }
            }

            return null;
        }

        public async Task UpdateCodeAsync(Group group, string code)
        {
            group.Code = code;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateSpecializationAsync(Group group, string specialization)
        {
            group.Specialization = specialization;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateOrientationAsync(Group group, string orientation)
        {
            group.Orientation = orientation;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePracticeTeacherFullNameAsync(Group group, string fullName)
        {
            group.PracticeTeacherFullName = fullName;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdatePracticeStartDateAsync(Group group, string startDate)
        {
            DateTime dateTime = DateTime.ParseExact(startDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            group.PracticeDateStart = dateTime;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdatePracticeEndDateAsync(Group group, string startDate)
        {
            DateTime dateTime = DateTime.ParseExact(startDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            group.PracticeDateEnd = dateTime;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> WriteOrEditAsync((string groupName, string groupCode, string groupSpecialization, string groupOrientation) groupInfo)
        {
            if (_dbContext.Groups.Any(g => g.Name == groupInfo.groupName)) // Изменение записи, если группа с таким названием уже существует в БД
            {
                List<Group> groupList =  await _dbContext.Groups
                    .Where(g => g.Name == groupInfo.groupName)
                    .ToListAsync();

                foreach (var item in groupList)
                {
                    item.Code = groupInfo.groupCode;
                    item.Specialization = groupInfo.groupSpecialization;
                    item.Orientation = groupInfo.groupOrientation;
                }

                await _dbContext.SaveChangesAsync();

                return 0;
            }
            else // Добавление новой группы.
            {
                Group newGroup = new Group()
                {
                    Name = groupInfo.groupName,
                    Code = groupInfo.groupCode,
                    Specialization = groupInfo.groupSpecialization,
                    Orientation = groupInfo.groupOrientation
                };

                await _dbContext.Groups.AddAsync(newGroup);
                await _dbContext.SaveChangesAsync();

                return 1;
            }
        }
    }
}
