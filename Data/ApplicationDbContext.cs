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
                .HasMany<Catalog>(c => c.Catalogs)
                .WithOne(e => e.User)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<Catalog>().HasData(
                new {Id = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), ParentID = new Guid("00000000-0000-0000-0000-000000000000"), Name = "Creating Digital Image" },
                new {Id = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Resources" },
                new {Id = new Guid("4a64b4f6-e007-4243-ac76-3a9ee7851381"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Evidence" },
                new {Id = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Graphic Products" },
                new {Id = new Guid("176278c3-a448-4efe-b73b-ba165b8dd45a"), ParentID = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), Name = "Primary Source" },
                new {Id = new Guid("4d4b72ec-1f51-4dfa-9d6c-a2385157883d"), ParentID = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), Name = "Secondary Source" },
                new {Id = new Guid("26e48c4c-7bff-4a26-a139-291a89a8648d"), ParentID = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), Name = "Process" },
                new {Id = new Guid("6e70a745-12c8-493c-ace5-0001c168b47c"), ParentID = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), Name = "Final Product" }
                );
        }

        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<User> Users { get; set; }
    }
}