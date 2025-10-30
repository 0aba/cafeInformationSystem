using System;
using cafeInformationSystem.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace cafeInformationSystem.Models.DataBase
{
    public static class DatabaseService
    {
        private static ApplicationDbContext? _context = null;

        public static ApplicationDbContext GetContext()
        {
            if (_context == null)
            {
                var connectionString = $"""
                Host={Environment.GetEnvironmentVariable("HOST_DB")};
                Port={Environment.GetEnvironmentVariable("PORT_DB")};
                Database={Environment.GetEnvironmentVariable("NAME_DB")};
                Username={Environment.GetEnvironmentVariable("USER_DB")};
                Password={Environment.GetEnvironmentVariable("PASSWORD_DB")};
                SSL Mode={Environment.GetEnvironmentVariable("SSLMODE_DB")}
                """;

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

                _context = new ApplicationDbContext(options);
                _context.Database.EnsureCreated();
            }
            
            return _context;
        }
    }
}
