using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace University.DLL.Sqlite
{
    public class UniversityDbContext : DbContext
    {
        public DbSet<Group> MyProperty { get; set; }
    }
}