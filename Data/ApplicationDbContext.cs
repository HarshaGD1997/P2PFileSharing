using Microsoft.EntityFrameworkCore;
using P2PFileSharing.Models;

namespace P2PFileSharing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileMetadata> Files { get; set; }
    }
}