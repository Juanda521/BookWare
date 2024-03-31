using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using System.Runtime.Serialization;

namespace tallerbiblioteca.Services
{
    public class PrestamosServices
    {
        private readonly BibliotecaDbContext _context;
        private PeticionesServices _peticionesServices;
        private ConfiguracionServices _configuracionServices;
        private EmailServices _emailServices;
        private ReservasServices _reservaServices;


        public PrestamosServices(BibliotecaDbContext bibliotecaDbContext,PeticionesServices peticionesServices,ConfiguracionServices configuracionServices,EmailServices emailServices,ReservasServices reservasServices){
            _context  = bibliotecaDbContext;
            _peticionesServices = peticionesServices;
            _configuracionServices = configuracionServices;
            _emailServices = emailServices;
            _reservaServices = reservasServices;

        }

       
        public async Task<List<Prestamo>> ObtenerPrestamos(){
            
            try
            {
                return await _context.Prestamos.OrderByDescending(p=>p.Fecha_inicio)
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Usuario)
                    .Include(p => p.Peticion)
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

       public async Task<List<Prestamo>> ObtenerPrestamosEnCurso()
{
    try
    {
        var prestamosEnEspera = await _context.Prestamos
            .Include(p => p.Peticion)
                .ThenInclude(p => p.Usuario)
            .Include(p => p.Peticion)
                .ThenInclude(p => p.Ejemplar)
                    .ThenInclude(p => p.Libro)
            .Where(p => p.Estado == "EN CURSO")
            .ToListAsync();

        return prestamosEnEspera;
    }
    catch (Exception ex)
    {
        // Manejo de la excepción
        Console.WriteLine($"Ocurrió un error al obtener los préstamos en espera: {ex.Message}");
        throw; // Relanza la excepción para que sea manejada en un nivel superior
    }
}



        public async Task<int> TotalPrestamos(){
            var prestamos = await ObtenerPrestamos();
            return prestamos.Count();
        }

       public List<Prestamo> BuscarPrestamos(string busqueda, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                Console.WriteLine("Vamos a buscar PRESTAMOS");
                Console.WriteLine(fechaInicio);
                Console.WriteLine(fechaFin);

                List<Prestamo> prestamos = _context.Prestamos
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Usuario)
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Ejemplar).ThenInclude(p => p.Libro)
                    .ToList();

                if (!string.IsNullOrEmpty(busqueda))
                {
                    prestamos = prestamos.Where(p =>
                        p.Peticion.Usuario.Name.ToLower().Contains(busqueda) ||
                        p.Peticion.Usuario.Numero_documento.ToString().Contains(busqueda) ||
                        p.Peticion.Usuario.Apellido.Contains(busqueda) ||
                        p.Peticion.Id_ejemplar.ToString().Contains(busqueda) || 
                        p.Peticion.Ejemplar.Libro.Nombre.ToLower().Contains(busqueda) ||
                        p.Estado.ToLower().Contains(busqueda))
                        .ToList();
                }

                if (fechaInicio != null && fechaFin != null)
                {
                    prestamos = prestamos.Where(p =>
    p.Fecha_inicio >= fechaInicio && p.Fecha_fin <=fechaFin )
    .ToList();

                }

                return prestamos;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al buscar préstamos: {ex.Message}");
                return new List<Prestamo>(); // O maneja el error de otra manera que consideres más adecuada para tu aplicación
            }
        }
        public async Task<bool> ValidarReservaExistente(int IdEjemplar)
        {
            return await _reservaServices.Buscar(IdEjemplar);
        }

        public async Task<Reserva> BuscarReservaExistente(int Id_ejemplar)
        {
            return await _reservaServices.BuscarReservaExistente(Id_ejemplar);
        }


        public async Task<Prestamo> Buscar(int id)
        {
            try
            {
                var peticion = await _context.Prestamos
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Usuario)
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Ejemplar)
                            .ThenInclude(e => e.Libro)
                    .SingleAsync(p => p.Id == id);

                return peticion;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al buscar el préstamo: {ex.Message}");
                return null; // O maneja el error de otra manera que consideres más adecuada para tu aplicación
            }
        }


        public async Task<List<Peticiones>> ObtenerPeticiones(){
            return await _peticionesServices.ObtenerpeticionesEnEspera();
        }

        public async Task<Peticiones>BuscarPeticion( int id){
            return await _peticionesServices.Buscar(id);
        }

        public async Task<ResponseModel> Registrar(Prestamo prestamo, ClaimsPrincipal User)
        {
            try
            {
                int status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_prestamo", _configuracionServices.ObtenerRolUserOnline(User));
                if (status == 200)
                {
                    prestamo.Fecha_inicio = obtenerFechaActual();
                    prestamo.Fecha_fin = ObtenerFechaFinal(prestamo.Fecha_inicio);
                    Console.WriteLine(prestamo.Id_peticion);
                    prestamo.Peticion = await _peticionesServices.Buscar(prestamo.Id_peticion);
                
                    prestamo.Peticion.Estado = "ACEPTADA";
                    prestamo.Peticion.Ejemplar.EstadoEjemplar = "EN PRESTAMO";
                    _context.Prestamos.Add(prestamo);
                    
                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Este es el correo al que se enviará el correo de confirmación de préstamo: " + prestamo.Peticion.Usuario.Correo);
                
                    // Enviamos correo al usuario confirmando su solicitud
                    _emailServices.SendEmail(_emailServices.EmailPrestamo(prestamo));
                }
                else
                {
                    Console.WriteLine("A mirar qué está fallando :( Código de estado: " + status);
                }
                return _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al registrar el préstamo: {ex.Message}");
                // Aquí podrías devolver un mensaje específico indicando que ocurrió un error al registrar el préstamo
                return new ResponseModel { Mensaje = "Ocurrió un error al registrar el préstamo.", Icono = "error" };
            }
        }


        public DateTime obtenerFechaActual(){
            return DateTime.Now;
        }

        public DateTime ObtenerFechaFinal(DateTime Fecha_inicio){
            return Fecha_inicio.AddDays(15);
        }

        public async Task<Prestamo> ObtenerPrestamo(int id)
        {
            try
            {
                return await _context.Prestamos.FindAsync(id);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al obtener el préstamo: {ex.Message}");
                return null; // O maneja el error de otra manera que consideres más adecuada para tu aplicación
            }
        }

       public async Task<ResponseModel> Editar(Prestamo prestamo, ClaimsPrincipal user, DateTime Fecha_fin)
        {
            try
            {
                Console.WriteLine("Llegamos a editar el préstamo");
                int status = _configuracionServices.ValidacionConfiguracionActiva("editar_prestamo", _configuracionServices.ObtenerRolUserOnline(user));
                var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
                if (status == 200)
                {
                    Console.WriteLine("Aquí ya va a editar el préstamo");

                    Console.WriteLine($"Esta es la fecha fin actual del préstamo: {prestamo.Fecha_fin}");
                    prestamo.Fecha_fin = Fecha_fin;
                    Console.WriteLine($"Esta es la fecha fin actualizada: {prestamo.Fecha_fin}");

                    _context.Update(prestamo);
                    await _context.SaveChangesAsync();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error al editar el préstamo: {ex.Message}");
                return new ResponseModel { Mensaje = "Ocurrió un error al editar el préstamo.", Icono = "error" };
            }
        }

        public async Task<List<Devolucion>> DevolverAutomatico(ClaimsPrincipal User)
        {
            try
            {
                Console.WriteLine("esta llegando aca?");
                List <Devolucion> devoluciones = new List<Devolucion>();
                Console.WriteLine("Llegando a DEVOLVER");
                var FechaActual = DateTime.Now;
                Console.WriteLine($"Esta es la fecha actual {FechaActual}");
                List<Prestamo> prestamos = await _context.Prestamos
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Usuario)
                    .ToListAsync();
                if (prestamos != null)
                {
                    var contador = 0; 
                    foreach(var Prestamo in prestamos)
                    {
                        Console.WriteLine("Entramos al for");
                        Console.WriteLine($"esta es la fecha fin del presamo{Prestamo.Fecha_fin}");
                        if (Prestamo.Estado=="EN CURSO" && FechaActual>Prestamo.Fecha_fin)
                        {
                            contador += 1;
                            Console.WriteLine("ESTE PRESTAMO YA PASÓ LA FECHA ");
                            Console.WriteLine("HAY " + contador + "Prestamos tardíos " +Prestamo.Peticion.Usuario.Name);
                                Prestamo.Estado = "EN MORA";
                                Devolucion devolucion = new Devolucion()
                                {
                                    Id_prestamo = Prestamo.Id,
                                    Prestamo = Prestamo,
                                    Fecha_devolucion = FechaActual,
                                    Observaciones = "El libro no se ha devuelto, está en mora",
                                    Estado = "PENDIENTE"

                                };
                                int rolUsuario = _configuracionServices.ObtenerRolUserOnline(User);
                                if (rolUsuario == 1)
                                {
                                    Console.WriteLine("VAMOS A DEVOLVER UN PRESTAMO DE ID " + Prestamo.Id);
                                    int status = await Registrar(devolucion, User);
                                    devoluciones.Add(devolucion);
                                    
                                }
                                else
                                {
                                    Console.WriteLine("ROL INVÁLIDO");
                                }

                        }
                        else {
                            Console.WriteLine("NO HAY PRESTAMOS PARA DEVOLVER");
                        }
                    }
                    
                }
                else
                {
                    Console.WriteLine("No hay nada para desactivar automaticamente");
                
                }
                return devoluciones;
            }
            catch(Exception)
            {
                return null; 
            }
        }

         public async Task<int> Registrar(Devolucion devolucion, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_devolucion", _configuracionServices.ObtenerRolUserOnline(User));

            var fechaActual = DateTime.Now;
            if (Status == 200)
            {
                    Console.WriteLine($"el id del prestamo asociado a la devolucion es: {devolucion.Id}");

                _context.Add(devolucion);
                await _context.SaveChangesAsync();
                var buscarD = _context.Devoluciones.Where(d => d.Estado == "PENDIENTE").ToList();

                if (buscarD.Count > 0)
                {
                    Console.WriteLine("SE HAN ENCONTRADO " + buscarD.Count + " EN MORA");
                    foreach (var devoluciones in buscarD)
                    {
         
                        bool existe = _context.Sanciones.Any(s => s.Id_devolucion == devoluciones.Id);
                        if (!existe) 
                        {
                            Sancion sanciones = new Sancion()
                            {
                                Id_devolucion = devoluciones.Id,
                                Devolucion = devoluciones,
                                Motivo_sancion = "No entregó el libro en la fecha estipulada ",
                                Fecha_Sancion = fechaActual,
                                Estado = "ACTIVA"
                            };
                            Console.WriteLine("Se acaba de sancionar correctamente a la devolucion con id: " + devoluciones.Id);
                            _context.Add(sanciones);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            Console.WriteLine("La devolución con el id " + devoluciones.Id + " Ya se encuentra sancionada");
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

       
    }
}