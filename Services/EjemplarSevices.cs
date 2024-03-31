using tallerbiblioteca.Models;
using Microsoft.Build.Framework;
//using tallerbiblioteca.Views.Usuarios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Context;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace tallerbiblioteca.Services
{
    public class EjemplarServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
        public LibrosServices _librosServices;
      
        public EjemplarServices(BibliotecaDbContext bibliotecaDbContext,LibrosServices librosServices,ConfiguracionServices configuracionServices)
        {
            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
            _librosServices = librosServices;
          
        }

        public async Task<Libro> BuscarLibro(int id){
            
            return await _librosServices.BuscarLibroAsync(id);
        }

        public int ValidacionConfiguracionActiva(string nombre,int Id_rol){
            return   Status = _configuracionServices.ValidacionConfiguracionActiva(nombre, Id_rol);
        }




        public async Task<int> Delete(int Id,ClaimsPrincipal User)
        {
            //si el estado que nos devolvio la validacion de la accion a realizar es correcta (status 200) podremos realizar la accion

            var Id_rol_string = User.FindFirst(ClaimTypes.Role).Value;  
            int Id_rol = Int32.Parse(Id_rol_string);

            Status = ValidacionConfiguracionActiva("Eliminar_ejemplar",Id_rol);
            Console.WriteLine("status:",Status);
            if (Status == 200)
            {
                Console.WriteLine("este es el id a eliminar:", Id);
                var ejemplar = await _context.Ejemplares.FindAsync(Id);
                if (ejemplar != null)
                {   
                    // Verificar si existen registros relacionados en la tabla de Peticiones
                    bool existenPeticionesRelacionadas = _context.Peticiones.Any(p => p.Id_ejemplar == ejemplar.Id);
                    // Si existen registros relacionados en cualquiera de las tablas, no eliminar
                    if (existenPeticionesRelacionadas)
                    {
                        Console.WriteLine("No se puede eliminar el Ejemplar porque existen registros relacionados.");
                        return 500;
                    }
                    var libro = _context.Libros.Find(ejemplar.Id_libro);

                    if(libro!=null){
                        libro.CantidadLibros--;
                        Console.WriteLine("hemos removido la cantidad");
                    }else{
                        Console.WriteLine("no esta encontrando libros");
                    }

                    _context.Ejemplares.Remove(ejemplar);
                    Console.WriteLine("ya debio haber eliminado");
                }
               await _context.SaveChangesAsync() ;
            }
            return Status;
        }

        public async Task<Ejemplar> BuscarEjemplar(int id){
             var ejemplar = await _context.Ejemplares.FindAsync(id);
             //validacion de objeto nullo coalesce (vscode) (compuesta) (si el objeto es diferente de nulo lo envia, en caso contrario crea uno)
             return ejemplar ??= new();
        }
        public async Task<List<Ejemplar>> ObtenerEjemplaresDisponibles()
        {
            try
            {
                var ejemplares = await _context.Ejemplares.Where(e=>e.EstadoEjemplar=="DISPONIBLE").Include(e => e.Libro).ToListAsync();
                return ejemplares;
            }
            catch (System.Exception)
            {

                throw;
            }

        }
        public async Task<List<Ejemplar>>ObtenerEjemplares(){
            try
            {
                  var ejemplares = await _context.Ejemplares.Include(e=>e.Libro).ToListAsync();
                  return ejemplares;
            }
            catch (System.Exception)
            {
                
                throw;
            }
          
        }

        public async Task<int> Registrar(Ejemplar ejemplar, ClaimsPrincipal User){

            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;
            int Id_rol = Int32.Parse(Id_rol_string);
           
            Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_ejemplar",Id_rol);
       
            switch (Status)
            {
                case 200:
                Console.WriteLine($"el id del libro a relacionar es: {ejemplar.Id_libro}");
                    var ejemplares = await ObtenerEjemplares();
                    if (ejemplares.Any(e=>e.Isbn_libro == ejemplar.Isbn_libro)){
                        Console.WriteLine("ya existe un ejemplar con este isbn");
                        return 203;
                    }
                    ejemplar.EstadoEjemplar  = "DISPONIBLE";
                    var Libro = await _context.Libros.FindAsync(ejemplar.Id_libro);
                    if (Libro!=null){
                        Libro.CantidadLibros++;
                        ejemplar.Libro = Libro;
                        
                    }else{
                        Console.WriteLine("no esta encontrando el libro");
                    }
                    _context.Ejemplares.Add(ejemplar);
                    await _context.SaveChangesAsync();
                break;
                
            }
            return Status;
        }

       public async Task<int> Edit(ClaimsPrincipal User,Ejemplar ejemplar){

            var Id_rol_string = User.FindFirst(ClaimTypes.Role).Value;
            int Id_rol = Int32.Parse(Id_rol_string);
            Status = _configuracionServices.ValidacionConfiguracionActiva("Editar_ejemplar", Id_rol);
            if(Status==200){
                
                if(ejemplar!=null){

                    Console.WriteLine(ejemplar.EstadoEjemplar);
                    if (ejemplar.EstadoEjemplar.Equals("DISPONIBLE"))
                    {
                        ejemplar.EstadoEjemplar = "NO_DISPONIBLE";
                    }
                    else
                    {
                        ejemplar.EstadoEjemplar = "DISPONIBLE";
                    }

                    _context.Update(ejemplar);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("ya debio haber editado");
                     Console.WriteLine("este es el estado nuevo" + ejemplar.EstadoEjemplar);
                }

            }
            return Status;
        }
    }
}