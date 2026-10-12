using Auth_Practise.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth_Practise.Data
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}
