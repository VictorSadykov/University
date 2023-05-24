using Microsoft.EntityFrameworkCore;
using University.Common;
using University.DLL.Sqlite.Entities;

namespace University.DLL.Sqlite
{
    public class UniversityDbContext : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        public UniversityDbContext()
        {
           
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DataConfig.DATA_FOLDER_PATH}UNIVERSITY.db");
        }
    }
}