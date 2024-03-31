using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
// using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Win32;
using tallerbiblioteca.Context;
using System.Globalization;

namespace tallerbiblioteca.Services
{
    public class DevolucionesServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
        private PrestamosServices _prestamosServices;
        private PeticionesServices _peticionesServices;

        public DevolucionesServices(BibliotecaDbContext bibliotecaDbContext, PeticionesServices peticionesServices, ConfiguracionServices configuracionServices, PrestamosServices prestamosServices)
        {

            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
            _prestamosServices = prestamosServices;
            _peticionesServices = peticionesServices;
        }

        public async Task<Devolucion> Buscar(int Id)
        {
            Console.WriteLine("Estamos en buscar devoluciones");
            var devolucion = await _context.Devoluciones.Include(p => p.Prestamo).SingleAsync(p => p.Id == Id);

            if (devolucion != null)
            {
                Console.WriteLine(devolucion);
                return devolucion;
            }
            return new();
        }

       
        public DateTime obtenerFechaActual()
        {
            return DateTime.Now;
        }

        public async Task<bool> BuscarDevolucionExistente(int id)
        {
            try
            {

                Console.WriteLine("Estamos en la función de validación");
                var devolucion = await _context.Devoluciones.Include(p => p.Prestamo).FirstOrDefaultAsync(d => d.Id_prestamo == id);

                if (devolucion != null)
                {
                    Console.WriteLine("Se encontró una devolución con el ID del préstamo");
                    return true;
                }
                else
                {
                    Console.WriteLine("No hay devoluciones registradas con este ID de préstamo");
                    return false;
                }





            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar devolución existente: {ex.Message}");
                return false;
            }

        }


        public List<Devolucion> BuscarDevolucion(string busqueda, DateTime? fechaDevolucion)
        {
            Console.WriteLine("vamos a buscar Devolucion");
            //  Console.WriteLine(fechaDevolucion);
            List<Devolucion> devoluciones = _context.Devoluciones
                .Include(p => p.Prestamo)
                    .ThenInclude(p => p.Peticion)
                    .ThenInclude(p => p.Usuario)
                .Include(p => p.Prestamo)
                    .ThenInclude(p => p.Peticion)
                    .ThenInclude(p => p.Ejemplar).ThenInclude(p => p.Libro)
                .ToList();

            if (!string.IsNullOrEmpty(busqueda))
            {
                devoluciones = devoluciones.Where(p =>
                    p.Observaciones.ToLower().Contains(busqueda) ||
                    p.Prestamo.Peticion.Usuario.Name.ToLower().Contains(busqueda) ||
                    p.Prestamo.Peticion.Id_ejemplar.ToString().Contains(busqueda) ||
                    p.Prestamo.Peticion.Ejemplar.Libro.Nombre.ToString().Contains(busqueda) ||
                    p.Prestamo.Id.ToString().Contains(busqueda))
                    .ToList();
            }

            if (fechaDevolucion != null)
            {
                DateTime fechaDevolucionValue = fechaDevolucion.Value.Date;
                // DateTime fechaFinValue = fechaFin.Value.Date.AddDays(1); // Incrementa un día para incluir la fecha de fin

                devoluciones = devoluciones.Where(p =>
                    p.Fecha_devolucion.Date >= fechaDevolucionValue)
                    .ToList();
            }

            return devoluciones;
        }

     

        public async Task<int> Registrar(Devolucion devolucion, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_devolucion", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {
                var fecha_devolucion = devolucion.Fecha_devolucion;
                Console.WriteLine(fecha_devolucion);
                var prestamo = await _prestamosServices.Buscar(devolucion.Id_prestamo);
                prestamo.Estado = "FINALIZADO";
                devolucion.Prestamo = prestamo;
                devolucion.Estado = "REALIZADA";
                devolucion.Prestamo.Peticion.Ejemplar.EstadoEjemplar = "DISPONIBLE";


                var reserva = await _prestamosServices.BuscarReservaExistente(devolucion.Prestamo.Peticion.Ejemplar.Id);
                if (reserva != null)
                {
                    Console.WriteLine("esta encontrando la reserva con el mismo ejemplar");
                    reserva.Estado = "ACEPTADA";
                    reserva.Ejemplar.EstadoEjemplar = "EN PETICION";

                    Console.WriteLine("vamos a registrar la peticion con  los datos de la reserva ya que el prestamo ha sido finalizado");
                    Peticiones peticion = new();
                    peticion.Id_usuario = reserva.IdUsuario;
                    peticion.Usuario = reserva.Usuario;

                    peticion.Id_ejemplar = reserva.IdEjemplar;
                    peticion.Ejemplar = reserva.Ejemplar;

                    peticion.Motivo = "Prestamo de libro que se habia reservado";

                    await _peticionesServices.Registrar(ObtenerRolUserOnline(User), peticion);


                }
                else
                {
                    Console.WriteLine("commo no hay reservas solo registramos la devolucion");
                    Console.WriteLine($"el id del prestamo asociado a la devolucion es: {devolucion.Id}");

                }
                _context.Add(devolucion);
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

        public int ObtenerRolUserOnline(ClaimsPrincipal User){
            return _configuracionServices.ObtenerRolUserOnline(User);
        }

        public async Task<List<Devolucion>> ObtenerDevoluciones()
        {
            return await _context.Devoluciones.OrderByDescending(p=>p.Fecha_devolucion).Include(p => p.Prestamo)
                .ThenInclude(p => p.Peticion)
                .ThenInclude(p => p.Usuario)
            .Include(p => p.Prestamo)
                .ThenInclude(p => p.Peticion)
                .ThenInclude(p => p.Ejemplar)
                .ThenInclude(p => p.Libro)
            .ToListAsync();
        }

         public async Task<List<Devolucion>> ObtenerDevolucionesPendientes()
        {
            return await _context.Devoluciones.OrderByDescending(p=>p.Fecha_devolucion).Where(p=>p.Estado == "PENDIENTE").Include(p => p.Prestamo)
                .ThenInclude(p => p.Peticion)
                .ThenInclude(p => p.Usuario)
            .Include(p => p.Prestamo)
                .ThenInclude(p => p.Peticion)
                .ThenInclude(p => p.Ejemplar)
                .ThenInclude(p => p.Libro)
            .ToListAsync();
        }

        public DateTime ObtenerFechaDevolucion(int Id)
        {

            var devolucionFecha = _context.Prestamos.Find(Id);
            return (DateTime)devolucionFecha.Fecha_fin;

        }

        public async Task<int> Editar(Devolucion devolucion, ClaimsPrincipal User, string texto)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Devolucion", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {
                if (devolucion.Estado == "REALIZADA")
                {
                    devolucion.Estado = "PENDIENTE";
                }
                else
                {
                    devolucion.Estado = "REALIZADA";
                }
                var prestamo = await _prestamosServices.Buscar(devolucion.Id_prestamo);
                prestamo.Estado = "FINALIZADO";
                devolucion.Prestamo = prestamo;

                _context.Update(devolucion);
                await EditarSancion(devolucion.Id,texto);
                await _context.SaveChangesAsync();

                // return View(devolucion);

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

        public async Task EditarSancion(int id,string Motivo){
            Console.WriteLine("editando la sancion...");
            var sancion  = await _context.Sanciones.Include(s=>s.Devolucion).FirstOrDefaultAsync(s=>s.Devolucion.Id == id);
            if(sancion!=null){
                sancion.Motivo_sancion  = Motivo;
                _context.Update(sancion);
                Console.WriteLine("Se ha editado la sancion");
            }
        }

        public async Task<int> Eliminar(int id, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Eliminar_devolucion", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {

                var devolucion = await _context.Devoluciones.FindAsync(id);
                var prestamo = await _prestamosServices.Buscar(devolucion.Id_prestamo);
                devolucion.Prestamo = prestamo;
                _context.Devoluciones.Remove(devolucion);
                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;
        }

        public ResponseModel MensajeRespuestaValidacionPermiso(int Status)
        {
            return _configuracionServices.MensajeRespuestaValidacionPermiso(Status);
        }


    }
}

