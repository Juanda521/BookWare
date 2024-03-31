using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Models;

namespace tallerbiblioteca.Context;

public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : base(options)
    {
    }

    // Agrega las propiedades DbSet para tus entidades/modelos
    public DbSet<Libro> Libros { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
   
    public DbSet<Rol> Rol { get; set; } 
    public DbSet<Permiso> Permisos { get; set; }
    public DbSet<Peticiones> Peticiones {get; set;}
    public DbSet<Configuracion> Configuracion {get; set;}
    public DbSet<Ejemplar>Ejemplares {get; set;}

    public DbSet<Genero> Genero { get; set; }
    public DbSet<Autor> Autores { get; set; }

    public DbSet<Prestamo>Prestamos {get; set;}
    public DbSet<GeneroLibro> GenerosLibros {get; set;}
    public DbSet<AutorLibro> AutoresLibros {get; set;}

    public DbSet<Publicaciones> Publicaciones {get; set;}
    public DbSet<Devolucion> Devoluciones { get; set; }
    public DbSet<Sancion> Sanciones {get; set;}
    public DbSet<Matriculados>Matriculados {get; set;}
    public DbSet<Reserva> Reserva { get; set;}






    // Puedes anular el método OnModelCreating para configurar relaciones y restricciones
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
              .Property(u => u.Numero_documento)
              .HasColumnType("bigint");
        // Configuraciones de entidades y relaciones
        base.OnModelCreating(modelBuilder);
    }
}

