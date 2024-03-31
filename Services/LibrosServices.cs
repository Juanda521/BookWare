using tallerbiblioteca.Models;
using Microsoft.Build.Framework;
//using tallerbiblioteca.Views.Usuarios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Context;
using System.Security.Claims;

namespace tallerbiblioteca.Services
{
    public class LibrosServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
 
        public LibrosServices(BibliotecaDbContext bibliotecaDbContext,ConfiguracionServices configuracionServices)
        {

            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
       
        }

        public async Task<int> Registrar(Libro libro, ClaimsPrincipal User)
        {
            try
            {
                Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_Libro", _configuracionServices.ObtenerRolUserOnline(User));
                Console.WriteLine("Estado de la validación: " + Status);

                if (Status == 200)
                {
                    _context.Add(libro);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("El libro se agregó correctamente.");
                }
                else
                {
                    Console.WriteLine("La validación no permitió guardar el libro.");
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar registrar el libro: {ex.Message}");
                // No se devuelve ningún código de error específico
            }

            return Status;
        }
        public async Task<int> CambiarEstado(Libro libro, ClaimsPrincipal user)
        {
            try
            {
                Console.WriteLine($"Estado actual del libro: {libro.Estado}");
                // Cambiar el estado del libro
                libro.Estado = (libro.Estado == "ACTIVO") ? "INACTIVO" : "ACTIVO";
                Console.WriteLine($"Estado actualizado del libro: {libro.Estado}");

                // Llamar al método Editar para guardar los cambios en la base de datos
                return await Editar(libro, user);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar cambiar el estado del libro: {ex.Message}");
                return 500; // Código de error interno del servidor
            }
        }
        public async Task<int> Editar(Libro libro, ClaimsPrincipal User)
        {
            try
            {
                Status = _configuracionServices.ValidacionConfiguracionActiva("Editar_Libro", _configuracionServices.ObtenerRolUserOnline(User));
                Console.WriteLine("Estado de la validación: " + Status);

                if (Status == 200)
                {
                    _context.Update(libro);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("El libro se editó correctamente desde los servicios.");
                }
                else
                {
                    Console.WriteLine("La validación no permitió editar el libro debido a los permisos.");
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar editar el libro: {ex.Message}");
                return 500; // Código de error interno del servidor
            }

            return Status;
        }
        public async Task<string> DevolverImagen(int libroId)
        {
            try
            {
                var libro = await _context.Libros.AsNoTracking().FirstOrDefaultAsync(l => l.Id == libroId);
                var imagen = libro.ImagenLibro;
                return imagen;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar obtener la imagen del libro: {ex.Message}");
                return null; // Puedes retornar un valor por defecto o manejar de otra manera el error.
            }
        }
        public List<Libro> ObtenerLibrosRelacionadosPorGenero(int idLibro)
        {
            try
            {
                var generosDelLibroActual = _context.GenerosLibros
                    .Where(gl => gl.Id_libro == idLibro)
                    .Select(gl => gl.Id_genero)
                    .ToList();

                var librosRelacionados = _context.GenerosLibros
                    .Where(gl => generosDelLibroActual.Contains(gl.Id_genero) && gl.Id_libro != idLibro)
                    .Select(gl => gl.Libro)
                    .Distinct()
                    .ToList();

                return librosRelacionados;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar obtener los libros relacionados por género: {ex.Message}");
                return new List<Libro>(); // Puedes retornar una lista vacía o manejar de otra manera el error.
            }
        }
        public Libro ConvertirImagen(byte[] LibroImagen, Libro libro, IFormFile? ImagenLibro)
        {
            try
            {
                using (var binaryReader = new BinaryReader(ImagenLibro.OpenReadStream()))
                {
                    LibroImagen = binaryReader.ReadBytes((int)ImagenLibro.Length);
                }
                libro.ImagenLibro = Convert.ToBase64String(LibroImagen);
                return libro;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Ocurrió un error al intentar convertir la imagen del libro: {ex.Message}");
                return null; // Puedes retornar un valor por defecto o manejar de otra manera el error.
            }
        }

        public ResponseModel MensajeRespuestaValidacionPermiso(int status){
            return _configuracionServices.MensajeRespuestaValidacionPermiso(status);
        }

       

        public async Task<List<Libro>> ObtenerLibros()
        {
            try
            {
                return await _context.Libros.ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de libros: {ex.Message}");
                return new List<Libro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public List<Libro> busqueda(string busqueda)
        {
            try
            {

               
                var libros = _context.Libros.Include(l => l.AutorLibros).ThenInclude(al => al.Autor).Include(l => l.GeneroLibros).ThenInclude(gl => gl.Genero).Where(l =>l.GeneroLibros.Any(gl => gl.Genero.NombreGenero.ToLower().Contains(busqueda.ToLower())) || l.AutorLibros.Any(al => al.Autor.NombreAutor.ToLower().Contains(busqueda.ToLower())) || l.Nombre.ToLower().Contains(busqueda)  || l.Id.ToString().Contains(busqueda) || l.CantidadLibros.ToString().Contains(busqueda)).ToList();
                Console.WriteLine("vamos a devolvwr los libros");
                return libros;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al buscar libros: {ex.Message}");
                return new List<Libro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<AutorLibro>> ObtenerAutoresRelacionados()
        {
            try
            {
                return await _context.AutoresLibros.Include(a => a.Autor).ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de autores relacionados: {ex.Message}");
                return new List<AutorLibro>(); // Devolvemos una lista vacía en caso de error
            }
        }


        public async Task<List<Autor>> ObtenerAutores()
        {
            try
            {
                return await _context.Autores.ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de autores: {ex.Message}");
                return new List<Autor>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<GeneroLibro>> ObtenerGenerosRelacionados()
        {
            try
            {
                return await _context.GenerosLibros.Include(a => a.Genero).ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de géneros relacionados: {ex.Message}");
                return new List<GeneroLibro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<Genero>> ObtenerGeneros()
        {
            try
            {
                return await _context.Genero.ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de géneros: {ex.Message}");
                return new List<Genero>(); // Devolvemos una lista vacía en caso de error
            }
        }

         public async Task<List<GeneroLibro>> ObtenerGenerosRelacionadosMovil(int id)
        {
            try
            {
                return await _context.GenerosLibros.Where(a=>a.Id_libro == id).Include(a => a.Genero).ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de géneros relacionados: {ex.Message}");
                return new List<GeneroLibro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<AutorLibro>> ObtenerAutoresRelacionadosMovil(int id)
        {
            try
            {
                return await _context.AutoresLibros.Where(a=>a.Id_libro == id).Include(a => a.Autor).ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al obtener la lista de autores relacionados: {ex.Message}");
                return new List<AutorLibro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        
       
        public async Task<List<Libro>> BusquedaporGeneros(int id)
        {
            try
            {
                var generos = await _context.GenerosLibros
                    .Where(g => g.Id_genero == id)
                    .Include(g => g.Genero)
                    .Include(g => g.Libro)
                    .ToListAsync();

                return generos.Select(g => g.Libro).ToList();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al buscar libros por género: {ex.Message}");
                return new List<Libro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<Libro>> BusquedaporAutores(int id)
        {
            try
            {
                var autores = await _context.AutoresLibros
                    .Where(a => a.Id_autor == id)
                    .Include(a => a.Autor)
                    .Include(a => a.Libro)
                    .ToListAsync();

                return autores.Select(a => a.Libro).ToList();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al buscar libros por autor: {ex.Message}");
                return new List<Libro>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<Libro> BuscarLibroAsync(int id)
        {
            try
            {
                return await _context.Libros.SingleOrDefaultAsync(p => p.Id == id) ?? new Libro();
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al buscar libro por ID: {ex.Message}");
                return new Libro(); // Devolvemos una nueva instancia de Libro en caso de error
            }
        }

        public async Task<List<Genero>> RelacionarGeneros(int id)
        {
            try
            {
                var generosRelacionados = await _context.GenerosLibros
                    .Where(a => a.Id_libro == id)
                    .ToListAsync();

                var generosMandar = new List<Genero>();
                foreach (var item in generosRelacionados)
                {
                    var genero = await _context.Genero.FirstOrDefaultAsync(g => g.Id == item.Id_genero);
                    if (genero != null)
                    {
                        generosMandar.Add(genero);
                    }
                }
                return generosMandar;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al relacionar géneros del libro: {ex.Message}");
                return new List<Genero>(); // Devolvemos una lista vacía en caso de error
            }
        }


        public async Task<List<Autor>> RelacionarAutores(int id)
        {
            try
            {
                var autoresRelacionados = await _context.AutoresLibros
                    .Where(a => a.Id_libro == id)
                    .ToListAsync();

                var autoresMandar = new List<Autor>();
                foreach (var item in autoresRelacionados)
                {
                    var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == item.Id_autor);
                    if (autor != null)
                    {
                        autoresMandar.Add(autor);
                    }
                }
                return autoresMandar;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al relacionar autores del libro: {ex.Message}");
                return new List<Autor>(); // Devolvemos una lista vacía en caso de error
            }
        }

        public async Task<List<Ejemplar>> RelacionarEjemplares(int id)
        {
            try
            {
                var ejemplaresRelacionados = await _context.Ejemplares
                    .Where(e => e.Id_libro == id)
                    .ToListAsync();

                return ejemplaresRelacionados;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Error al relacionar ejemplares del libro: {ex.Message}");
                return new List<Ejemplar>(); // Devolvemos una lista vacía en caso de error
            }
        }
        public async Task<List<Libro>> ObtenerLibrosPdf()
        {
            var libros = await ObtenerLibros();
            List<Libro> librosList = new List<Libro>();

            foreach (var libro in libros)
            {
                libro.Ejemplares = await RelacionarEjemplares(libro.Id);
                libro.AutoresRelacionados = await ObtenerAutoresRelacionadosMovil(libro.Id);
                libro.GenerosRelacionados = await ObtenerGenerosRelacionadosMovil(libro.Id);

                // Assuming Ejemplares, AutoresRelacionados, and GenerosRelacionados are lists of objects that can be directly assigned
                // If they need to be converted to specific types, you would need to adjust this part accordingly

                librosList.Add(libro);
            }

            return librosList;
        }


    }
}