using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;

namespace TaskManagement.DbContexts
{
    public static class DataSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Task Status Lookups (MajorCode = 1)
            modelBuilder.Entity<Lookup>().HasData(
                new Lookup { Id = 1, MajorCode = 1, MinorCode = 0, Name = "Task Status" },
                new Lookup { Id = 2, MajorCode = 1, MinorCode = 1, Name = "Initiated" },
                new Lookup { Id = 3, MajorCode = 1, MinorCode = 2, Name = "In Progress" },
                new Lookup { Id = 4, MajorCode = 1, MinorCode = 3, Name = "Completed" },
                new Lookup { Id = 5, MajorCode = 1, MinorCode = 4, Name = "Cancelled" }
            );
        }
    }
}
