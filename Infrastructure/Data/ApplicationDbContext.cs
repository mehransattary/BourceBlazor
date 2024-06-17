using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Nomad> Nomads { get; set; }
        public DbSet<NomadDate> NomadDates { get; set; }
        public DbSet<NomadAction> NomadActions { get; set; }
        public DbSet<Hajm> Hajm { get; set; } = default!;
        public DbSet<Formol> Formols { get; set; } = default!;
    }
}
