using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Win32;
using tallerbiblioteca.Context;



namespace tallerbiblioteca.Services
{
    public class SancionesServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
        private DevolucionesServices _devolucionesServices;

        public SancionesServices(BibliotecaDbContext bibliotecaDbContext, ConfiguracionServices configuracionServices, DevolucionesServices devolucionesServices)
        {

            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
            _devolucionesServices = devolucionesServices;
        }

        // public async Task<Sancion> BarraBusqueda(int busqueda)
        // {
        //     if(busqueda!=null)
        //     {
        //         busqueda=await _context.Sanciones.Where(d=>d.Id.Containts(busqueda)).ToListAsync();
        //     }
        // }

        public async Task<int> Registrar(Sancion sancion, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_sancion", _configuracionServices.ObtenerRolUserOnline(User));
            if (Status == 200)
            {

                // var fecha_sancion = ObtenerFechaSancion(sancion.Id);
                var devolucion = await _devolucionesServices.Buscar(sancion.Id_devolucion);
                sancion.Devolucion=devolucion;
                _context.Add(sancion);
                await _context.SaveChangesAsync();

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);
            }
            return Status;


        }

        public async Task<List<Devolucion>>ObtenerDevoluciones(){
            return await _devolucionesServices.ObtenerDevolucionesPendientes();
        }

        public async Task<List<Sancion>> ObtenerSancionesPdf()
        {
            try
            {
                return await _context.Sanciones.Include(p => p.Devolucion)
                                               .ThenInclude(p => p.Prestamo)
                                               .ThenInclude(p => p.Peticion)
                                               .ThenInclude(p => p.Usuario)
                                                .Include(p => p.Devolucion)
                                               .ThenInclude(p => p.Prestamo)
                                               .ThenInclude(p => p.Peticion)
                                               .ThenInclude(p => p.Ejemplar)
                                               .ThenInclude(p => p.Libro)
                                           .ToListAsync();
            
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al obtener los préstamos: {ex.Message}");
                return null; // O maneja el error de otra manera que consideres más adecuada para tu aplicación
            }
        }
        public async Task<List<Sancion>> ObtenerSanciones()
        {
            return await _context.Sanciones.Include(p => p.Devolucion)
                                                .ThenInclude(p => p.Prestamo)
                                                .ThenInclude(p => p.Peticion)
                                                .ThenInclude(p => p.Usuario)
                                                 .Include(p => p.Devolucion)
                                                .ThenInclude(p => p.Prestamo)
                                                .ThenInclude(p => p.Peticion)
                                                .ThenInclude(p => p.Ejemplar)
                                                .ThenInclude(p => p.Libro)
                                            .ToListAsync();
        }

        public async Task<Sancion> Buscar(int Id)
        {

            var sancion = await _context.Sanciones
                                .Include(s => s.Devolucion)
                                    .ThenInclude(d => d.Prestamo)
                                        .ThenInclude(p => p.Peticion)
                                            .ThenInclude(p => p.Usuario)
                                .Include(s => s.Devolucion)
                                    .ThenInclude(d => d.Prestamo)
                                        .ThenInclude(p => p.Peticion)
                                            .ThenInclude(p => p.Ejemplar)
                                                .ThenInclude(e => e.Libro)
                                .FirstOrDefaultAsync(s => s.Id == Id);
            return sancion;

        }

        public async Task<int> Editar(Sancion sancion, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_sancion", _configuracionServices.ObtenerRolUserOnline(User));
            if (Status == 200)
            {

                if (sancion.Estado == "ACTIVA")
                {
                    sancion.Estado = "FINALIZADA";
                    sancion.Devolucion.Prestamo.Peticion.Usuario.Estado  = "ACTIVO";
                }
                else
                {
                    sancion.Estado = "ACTIVA";
                    sancion.Devolucion.Prestamo.Peticion.Usuario.Estado  = "SUSPENDIDO";
                }
               
                // sancion.Estado = "FINALIZADA";
               
               
                _context.Update(sancion);
                await _context.SaveChangesAsync();
                // ViewData["Id_devolucion"] = new SelectList(_context.Devoluciones, "Id", "Id", sancion.Id_devolucion);
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);
            }
            return Status;


        }

        public async Task<int> Eliminar(int id, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Eliminar_sancion", _configuracionServices.ObtenerRolUserOnline(User));
            if (Status == 200)
            {
                var sancion = await _context.Sanciones.FindAsync(id);
                _context.Sanciones.Remove(sancion);
                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Index));

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);
            }
            return Status;


        }



    }
}

