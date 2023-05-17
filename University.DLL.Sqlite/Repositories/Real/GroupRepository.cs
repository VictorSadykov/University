﻿using Microsoft.EntityFrameworkCore;
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

        public async Task Add(string groupName)
        {
            await _dbContext.Groups
                .AddAsync(new Group { Name= groupName });

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddLesson(Group group, Lesson lesson)
        {
            Group foundGroup = await _dbContext.Groups
                 .Include(g => g.Lessons)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Lessons.Add(lesson);

            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetLessonSchedule(Group group)
        {
            Group foundGroup = await _dbContext.Groups
                 .Include(g => g.Lessons)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Lessons = new List<Lesson>();
        }

        public async Task ResetExamSchedule(Group group)
        {
            Group foundGroup = await _dbContext.Groups
                 .Include(g => g.Exams)
                 .Where(g => g.Name == group.Name)
                 .FirstOrDefaultAsync();

            foundGroup.Exams = new List<Exam>();
        }

        public async Task<Group> FindByName(string groupName)
        {
            return await _dbContext.Groups
                .Where(g => g.Name == groupName)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Group>?> GetAllGroupsByNameAsync(string groupName)
        {
            return await _dbContext.Groups
                .Where(g => g.Name == groupName)
                .ToListAsync();
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
