using FerioBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FerioBackend.Data
{
    public class FerioDbContext : DbContext
    {
        public FerioDbContext(DbContextOptions<FerioDbContext> options) : base(options) { }

        //  tablas en la base de datos
        public DbSet<Stand> Stands { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Mensaje> Mensajes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<StandCategoria> StandCategoria { get; set; }  



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Stand>()
                .HasOne(s => s.Usuario) 
                .WithOne(u => u.Stand) 
                .HasForeignKey<Usuario>(u => u.StandId) 
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StandCategoria>()
                .HasKey(sc => new { sc.StandId, sc.CategoriaId });

            modelBuilder.Entity<StandCategoria>()
                .HasOne(sc => sc.Stand)
                .WithMany(s => s.StandCategoria)
                .HasForeignKey(sc => sc.StandId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StandCategoria>()
                .HasOne(sc => sc.Categoria)
                .WithMany(c => c.StandCategoria)
                .HasForeignKey(sc => sc.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Stand)
                .WithOne(s => s.Usuario)
                .HasForeignKey<Usuario>(u => u.StandId)
                .OnDelete(DeleteBehavior.SetNull); 


            base.OnModelCreating(modelBuilder);
        }

    }
}
