using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using webapp.Areas.Identity.Data;
using webapp.Models;

namespace webapp.Data;

public class webappContext : IdentityDbContext<webappUser>
{
    public webappContext(DbContextOptions<webappContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyAddress> PropertyAddresses { get; set; }
    public DbSet<PropertyDetail> PropertyDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
