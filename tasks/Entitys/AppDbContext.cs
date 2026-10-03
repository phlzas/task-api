using Microsoft.EntityFrameworkCore;
using tasks.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace tasks.Entitys
{
    public class AppDbContext : DbContext

    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }

        public DbSet<TaskItem> TaskItems { get; set; }
    }
}
