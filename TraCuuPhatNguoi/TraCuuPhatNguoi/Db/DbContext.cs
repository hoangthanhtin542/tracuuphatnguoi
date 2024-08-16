using Microsoft.EntityFrameworkCore;
using TraCuuPhatNguoi.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tracuu> Tracuu { get; set; }
}
