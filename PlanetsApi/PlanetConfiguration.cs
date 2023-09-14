using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlanetConfiguration : IEntityTypeConfiguration<Planet>
{
    public void Configure(EntityTypeBuilder<Planet> builder)
    {
        // Ignore these properties
        builder.Ignore(p => p.Residents);
        builder.Ignore(p => p.Films);
    }
}