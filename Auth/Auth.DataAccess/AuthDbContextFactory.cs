using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.DataAccess;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connection = "Host=localhost;Port=5432;Database=MicroAuthasd;Username=postgres;Password=postgres";
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        optionsBuilder.UseNpgsql(connection, opt => opt.CommandTimeout((int) TimeSpan.FromMinutes(2).TotalSeconds));

        return new AuthDbContext(optionsBuilder.Options);
    }
}