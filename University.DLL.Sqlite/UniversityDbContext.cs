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
        public DbSet<Corpus> Corpuses { get; set; }

        public UniversityDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DataConfig.DATA_FOLDER_PATH}UNIVERSITY.db");
        }
    }
}