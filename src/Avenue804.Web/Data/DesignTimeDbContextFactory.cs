using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Avenue804.Web.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var asmDir = Path.GetDirectoryName(typeof(DesignTimeDbContextFactory).Assembly.Location);
            if (!string.IsNullOrEmpty(asmDir) && File.Exists(Path.Combine(asmDir, "appsettings.json")))
                basePath = asmDir;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Set it in appsettings or environment.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
