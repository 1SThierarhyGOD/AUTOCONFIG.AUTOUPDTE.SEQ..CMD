using Microsoft.EntityFrameworkCore;

namespace ManageMetadataUpdates
{
    internal class MetadataContext : DbContext
    {
        public MetadataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MetadataRecord> MetadataRecords { get; set; } = null!;
    }
}
