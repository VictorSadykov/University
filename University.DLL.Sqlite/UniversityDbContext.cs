using Microsoft.EntityFrameworkCore;
using University.Configuration;
using University.DLL.Sqlite.Entities;

namespace University.DLL.Sqlite
{
    public class UniversityDbContext : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Corpus> Corpuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DataConfig.DataFolderPath}UNIVERSITY.db");
        }
    }
}