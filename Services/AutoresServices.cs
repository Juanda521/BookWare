using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;


namespace tallerbiblioteca.Services
{
    public class AutoresServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
        private LibrosServices _librosServices;

        public AutoresServices(BibliotecaDbContext bibliotecaDbContext, ConfiguracionServices configuracionServices, LibrosServices librosServices)
        {

            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
            _librosServices = librosServices;
        }

        public async Task<Autor> Buscar(int Id)
        {
            var autor = await _context.Autores.Where(a => a.Id == Id ).FirstOrDefaultAsync();
            if (autor != null)
            {
                return autor;
            }
            return new();
        }


        public async Task<List<AutorLibro>>ObtenerLibros(){
            return await _context.AutoresLibros.Include(l=>l.Libro).ToListAsync();
        }

        public async Task<List<Autor>>BusquedaporLibros(int id){

            var libros =   await _context.AutoresLibros.Where(a=>a.Id_libro == id).Include(a=>a.Libro).Include(a=>a.Autor).ToListAsync();
            List<Autor> Autores  = new List<Autor>();
            foreach (var item in libros)
            {
                Autores.Add(item.Autor);
            }
            return Autores;
           
        }



        // public async Task<int> Registrar(Autor autor, ClaimsPrincipal User)
        // {
        //     var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;
        //     int Id_rol = Int32.Parse(Id_rol_string);

            

        //     int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_autor", _configuracionServices.ObtenerRolUserOnline(User));

        //     switch(Status){
        //         case 200:
        //         Console.WriteLine($"el id de los libros a relacionar:{autor.Id}");
        //         int idLibro = _autoresServices.AutorRelacionadosPorLibro(autor.Id);
                
        //         if(libro != null){
        //             var nombre_autor = autor.NombreAutor;
        //             _context.Add(autor);
        //             await _context.SaveChangesAsync();
                    
        //         }else
        //         {
        //             Console.WriteLine("A mirar que esta fallando :(" + Status);

        //         }
        //         return Status;
        //     }
            

        // }

        public async Task<int> Registrar(Autor autor, ClaimsPrincipal User)
        {
            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;
            int Id_rol = Int32.Parse(Id_rol_string);

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_Autor", _configuracionServices.ObtenerRolUserOnline(User));


            
            if(Status == 200)
            {
                autor.Estado = "ACTIVO";                
                _context.Add(autor);
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

        

        public List<Autor>BuscarAutores(string busqueda ){
            return  _context.Autores.Where(l=>l.NombreAutor.ToLower().Contains(busqueda) || l.Estado.ToLower().Contains(busqueda) ).ToList();
        }

        public async Task<List<Autor>>ObtenerAutores(){
            return await _context.Autores.ToListAsync();
        }

        public async Task<int> Editar(Autor autor, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Autor", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {

                autor.Estado = "ACTIVO";
                // var estado_autor= autor.Estado;


                Console.WriteLine("Esta guardando las vueltas");
                _context.Update(autor);
                await _context.SaveChangesAsync();
                // return View(devolucion);

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

        // public async Task<int> Eliminar(int id, ClaimsPrincipal User)
        // {

        //     int Status = _configuracionServices.ValidacionConfiguracionActiva("Eliminar_autor", _configuracionServices.ObtenerRolUserOnline(User));


        //     if (Status == 200)
        //     {
                
        //         var autor = await _context.Autores.FindAsync(id);
        //         _context.Autores.Remove(autor);
        //         await _context.SaveChangesAsync();
        //         // return RedirectToAction(nameof(Index));
        //     }
        //     else
        //     {
        //         Console.WriteLine("A mirar que esta fallando :(" + Status);

        //     }
        //     return Status;
        // }

        public ResponseModel MensajeRespuestaValidacionPermiso(int Status)
        {
            return _configuracionServices.MensajeRespuestaValidacionPermiso(Status);
        }

        public int Desactivar(int id, ClaimsPrincipal User)
        {
            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;

            int Id_rol = Int32.Parse(Id_rol_string);
            //debe ser el mismo  nombre de la tabla permisos
            this.Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Autor", Id_rol);
            //si el estado que nos devolvio la validacion de la accion a realizar es correcta (status 200) podremos realizar la accion
            var autor = _context.Autores.Find(id);
            if (Status == 200)
            {
                if (autor != null)
                {
                    autor.Estado = "INHABILITADO";
                    _context.SaveChanges();
                }
            }
            return Status;

        }

         public int Activar(int id, ClaimsPrincipal User)
        {
            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;

            int Id_rol = Int32.Parse(Id_rol_string);
            //debe ser el mismo  nombre de la tabla permisos
            this.Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Autor", Id_rol);
            //si el estado que nos devolvio la validacion de la accion a realizar es correcta (status 200) podremos realizar la accion
            var autor = _context.Autores.Find(id);
            if (Status == 200)
            {
                if (autor != null)
                {
                    autor.Estado = "ACTIVO";
                    _context.SaveChanges();
                }
            }
            return Status;

        }


    }
}


