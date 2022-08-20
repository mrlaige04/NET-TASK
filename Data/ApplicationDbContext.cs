using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NET_TASK.Models;


namespace NET_TASK.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(c => c.Catalogs)
                .WithOne(e => e.User)
                .HasForeignKey(c => c.UserID);          
        }

        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<User> Users { get; set; }
    }
}