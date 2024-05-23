using Domain.Entittes.Bource;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BourceBlazor.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Nomad> Nomads { get; set; }
        public DbSet<NomadDate> NomadDates { get; set; }
        public DbSet<NomadAction> NomadActions { get; set; }
    }
}
