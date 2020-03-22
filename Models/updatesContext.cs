using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UpdateProvider.Models
{
    public partial class UpdatesContext : DbContext
    {
        public UpdatesContext()
        {
        }

        public UpdatesContext(DbContextOptions<UpdatesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<History> History { get; set; }
        public new virtual DbSet<Update> Update { get; set; }

        public virtual Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry UpdateDbContext(object obj) {
            return base.Update(obj);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<History>(entity =>
            {
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.DeliveredVersionNavigation)
                    .WithMany(p => p.HistoryDeliveredVersionNavigation)
                    .HasForeignKey(d => d.DeliveredVersion)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.PreviousVersionNavigation)
                    .WithMany(p => p.HistoryPreviousVersionNavigation)
                    .HasForeignKey(d => d.PreviousVersion);
            });

            modelBuilder.Entity<Update>(entity =>
            {
                entity.Property(e => e.Beta);
                
                entity.Property(e => e.Latest);

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Build).HasColumnName("Build");

                entity.Property(e => e.Major).HasColumnName("Major");

                entity.Property(e => e.Minor).HasColumnName("Minor");

                entity.Property(e => e.Revision).HasColumnName("Revision");

                entity.Property(e => e.PreviousBuild).HasColumnName("Previous Build");

                entity.Property(e => e.PreviousMajor).HasColumnName("Previous Major");

                entity.Property(e => e.PreviousMinor).HasColumnName("Previous Minor");

                entity.Property(e => e.PreviousRevision).HasColumnName("Previous Revision");

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(128);

                entity.Property(e => e.Product)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
