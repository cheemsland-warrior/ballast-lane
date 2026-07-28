using Microsoft.EntityFrameworkCore;
using BallastLaneApi.Models;

namespace BallastLaneApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pothole> Potholes { get; set; } // Added Potholes

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var isPostgres = Database.IsNpgsql();

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Email).IsRequired();
                b.Property(u => u.DisplayName).IsRequired(false);
                b.Property(u => u.CreatedDate).HasDefaultValue(DateTime.Now);
                if (isPostgres)
                {
                    b.Property(u => u.PasswordHash).HasColumnType("bytea");
                    b.Property(u => u.PasswordSalt).HasColumnType("bytea");
                }
            });

            modelBuilder.Entity<Pothole>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Description).HasMaxLength(500);

                if (isPostgres)
                {
                    // Explicitly tell Postgres these are high-precision map values
                    b.Property(p => p.Latitude).HasColumnType("double precision").IsRequired();
                    b.Property(p => p.Longitude).HasColumnType("double precision").IsRequired();
                }
                else
                {
                    b.Property(p => p.Latitude).IsRequired();
                    b.Property(p => p.Longitude).IsRequired();
                }

                b.Property(p => p.Status).HasDefaultValue("Reported");
                b.Property(p => p.CreatedDate).HasDefaultValue(DateTime.Now);

                // Establish one-to-many relationship (One user can report many potholes)
                b.HasOne(p => p.User)
                 .WithMany()
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}