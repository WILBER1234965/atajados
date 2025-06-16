using atajados.Models;
using Microsoft.EntityFrameworkCore;

namespace atajados.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Atajado> Atajados { get; set; }
        public DbSet<Seguimiento> Seguimientos { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índice único por si algún motor ignora el atributo
            modelBuilder.Entity<Item>()
                        .HasIndex(i => i.Numero)
                        .IsUnique();

            // Atajado (1) --- (N) Seguimientos
            modelBuilder.Entity<Seguimiento>()
                        .HasOne(s => s.Atajado)
                        .WithMany(a => a.Seguimientos)
                        .HasForeignKey(s => s.AtajadoId)
                        .OnDelete(DeleteBehavior.Cascade);

            // Item (1) --- (N) Seguimientos
            modelBuilder.Entity<Seguimiento>()
                        .HasOne(s => s.Item)
                        .WithMany()
                        .HasForeignKey(s => s.ItemId)
                        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
