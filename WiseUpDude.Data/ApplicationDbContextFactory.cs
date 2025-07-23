using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace WiseUpDude.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try multiple possible locations for appsettings files
            string[] possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "../WiseUpDude/appsettings.Development.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "../WiseUpDude/appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "WiseUpDude", "appsettings.Development.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "WiseUpDude", "appsettings.json")
            };

            var builder = new ConfigurationBuilder();
            bool found = false;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    builder.AddJsonFile(path, optional: false);
                    found = true;
                }
            }
            if (!found)
            {
                throw new FileNotFoundException("Could not find appsettings.Development.json or appsettings.json in any known location.");
            }

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string not found in configuration.");
            }
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}