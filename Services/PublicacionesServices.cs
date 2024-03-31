using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
using System.ComponentModel.DataAnnotations;



namespace tallerbiblioteca.Services
{
    public class PublicacionesServices
    {
        private readonly BibliotecaDbContext _context;
        private ConfiguracionServices _configuracionServices;
        public PublicacionesServices(BibliotecaDbContext context, ConfiguracionServices configuracionServices)
        {
            _context = context;
            _configuracionServices = configuracionServices;
        }
        public async Task<List<Models.Publicaciones>> ObtenerPublicaciones()
        {
            var fechaActual = DateTime.Now;
            var publi = _context.Publicaciones.Where(p => p.Estado == "ACTIVO");
            foreach (var publica in publi)
            {
                if (fechaActual > publica.FechaFin)
                {
                    publica.Estado = "INACTIVO";
                    
                }
            }
            var publicaciones = await _context.Publicaciones
           .Where(p => p.Estado == "ACTIVO")
           .ToListAsync();

            await _context.SaveChangesAsync();

            return publicaciones;
        }
        public async Task<List<Models.Publicaciones>> enviarcambiar(int id)
        {
            try
            {
                var publicaciones = await _context.Publicaciones.Where(m => m.Id == id).ToListAsync();

                if (publicaciones != null)
                {
                    return publicaciones;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("La publicación no fue encontrada");
            }
            return null;
        }
        public async Task<ResponseModel> CambiarEstado(int id, ClaimsPrincipal User)
        {

            int status = _configuracionServices.ValidacionConfiguracionActiva("Desactivar_publicacion", _configuracionServices.ObtenerRolUserOnline(User));
            if (status == 200)
            {
                var publicacion = await _context.Publicaciones.FirstOrDefaultAsync(p => p.Id == id);
                if (publicacion != null)
                {
                    publicacion.Estado = "INACTIVO";
                    await _context.SaveChangesAsync();
                }
            }
            var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            return resultado;
        }
        public async Task<Models.Publicaciones> ObtenerEditar(int id)
        {
            try
            {
                Console.WriteLine("Intentando encontrar");
                var publicaciones = await _context.Publicaciones.FindAsync(id);
                return publicaciones;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("FALLO EN SERVICES");
                Console.WriteLine($"Mensaje de excepción: {ex.Message}");
                throw new Exception("El ID no existe", ex); // Se agrega la causa original
            }
        }
        public async Task<ResponseModel> Editar(Models.Publicaciones publicaciones, ClaimsPrincipal User)
        {
            try
            {
                var status = _configuracionServices.ValidacionConfiguracionActiva("Editar_publicacion", _configuracionServices.ObtenerRolUserOnline(User));
                if (status == 200)
                {
                    var existingPublicacion = await _context.Publicaciones.FindAsync(publicaciones.Id);
                    if (existingPublicacion != null)
                    {
                        existingPublicacion.Tipo = publicaciones.Tipo;
                        existingPublicacion.Nombre = publicaciones.Nombre;
                        existingPublicacion.Descripcion = publicaciones.Descripcion;
                        existingPublicacion.FechaInicio = publicaciones.FechaInicio;
                        existingPublicacion.FechaFin = publicaciones.FechaFin;
                        _context.Update(existingPublicacion);
                        await _context.SaveChangesAsync();
                    }
                }

                var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
                return resultado;
            }
            catch (Exception ex)
            {
                // Logea el error aquí si es necesario
                Console.WriteLine($"Error al editar la publicación: {ex.Message}");
                return new ResponseModel { Mensaje = "Hubo un error al editar la publicación." };
            }
        }
        public async Task<ResponseModel> Crear(Models.Publicaciones publicaciones, ClaimsPrincipal User)
        {
            Console.WriteLine("Llegando a crear Publicacion");
            int status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_publicacion", _configuracionServices.ObtenerRolUserOnline(User));
            if (status == 200)
            {
                Console.WriteLine("TODO FINE");
                _context.Publicaciones.Add(publicaciones); 
            }
            var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            await _context.SaveChangesAsync();
            return resultado;

        }
        public async Task<List<Models.Publicaciones>> Buscar(string busqueda)
        {
            return _context.Publicaciones.Where(p => p.Estado == "ACTIVO" && (p.Nombre.Contains(busqueda)|| p.Tipo.Contains(busqueda))).ToList();
        }
        public async Task<List<Models.Publicaciones>> buscarfecha(DateTime? fechainicio, DateTime? fechafin)
        {
            return _context.Publicaciones.Where(p => p.Estado == "ACTIVO" && (p.FechaInicio >= fechainicio && p.FechaInicio <= fechafin)).ToList();
        }
        public async Task<List<Models.Publicaciones>> Desactivadas()
        {
            return _context.Publicaciones.Where(p => p.Estado == "INACTIVO").ToList();
        }
    }


}