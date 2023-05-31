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
    public class TeacherRepository : ITeacherRepository
    {
        private readonly UniversityDbContext _dbContext;

        public TeacherRepository(UniversityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Teacher teacher)
        {
            var entry = _dbContext.Entry(teacher);
            if (entry.State == EntityState.Detached)
            {
                await _dbContext.Teachers
                    .AddAsync(teacher);

            }

            
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetLessonScheduleAsync(Teacher teacher)
        {
            Teacher? foundTeacher = await _dbContext.Teachers
                 .Include(t => t.Lessons)
                 .Where(t => t.FirstName == teacher.FirstName &&
                             t.LastName == teacher.LastName &&
                             t.SecondName == teacher.SecondName)
                 .FirstOrDefaultAsync();

            foundTeacher.Lessons = new List<Lesson>();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Teacher?> FindByIdAsync(int id)
        {
            return await _dbContext.Teachers.Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public List<(int, DayOfWeek)> GetWorkingDays(string fullName)
        {
            Teacher? foundTeacher = FindByFullName(fullName);

            List<(int WeekNumber, DayOfWeek DayNumber)> weekNumberAndDays = new List<(int, DayOfWeek)>();

            

            foreach (var item in foundTeacher.Lessons)
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

        public Teacher? FindByFullName(string fullName)
        {
            try
            {
                string[] strings;
                if (fullName.Contains("."))
                {
                    fullName = fullName
                        .Replace(".", "");
                }

                strings = fullName
                            .Split(" ");



                if (strings.Length == 2)
                {
                    string firstNameLetter = strings[1][0].ToString();
                    string secondNameLetter = strings[1][1].ToString();
                    strings[1] = firstNameLetter;
                    strings = strings.Append(secondNameLetter).ToArray();
                }

                string firstName = strings[1].ToLower();
                string lastName = strings[0].ToLower();
                string secondName = strings[2].ToLower();

                foreach (var item in _dbContext.Teachers.Include(t => t.Lessons).Include(t => t.Exams))
                {
                    if (item.FirstName.ToLower() == firstName &&
                        item.LastName.ToLower() == lastName &&
                        item.SecondName.ToLower() == secondName)
                    {
                        return item;
                    }
                }

                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public List<Teacher> FindAllByLastName(string lastName)
        {
            var list = new List<Teacher>();
            foreach (var item in _dbContext.Teachers)
            {
                if (item.LastName.ToLower() == lastName.ToLower())
                {
                    list.Add(item);
                }
            }

            return list;

            
        }


    }
}
