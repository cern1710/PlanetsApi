using Microsoft.EntityFrameworkCore;

class PlanetDb : DbContext
{
    public PlanetDb(DbContextOptions<PlanetDb> options)
        : base(options) { }

    public DbSet<Planet> Planets => Set<Planet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanetConfiguration());
    }
}