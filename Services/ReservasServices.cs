using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
namespace tallerbiblioteca.Services
{

    public class ReservasServices
    {
        private readonly BibliotecaDbContext _context;
        private EjemplarServices _ejemplarservices;
        private UsuariosServices _usuariosServices;
        private ConfiguracionServices _configuracionServices;
       
        public ReservasServices(BibliotecaDbContext context, EjemplarServices ejemplarservices, UsuariosServices usuariosServices, ConfiguracionServices configuracionServices)
        {
            _context = context;
            _ejemplarservices = ejemplarservices;
            _usuariosServices = usuariosServices;
            _configuracionServices = configuracionServices;
     
        }
        public async Task<List<Ejemplar>> obtenerEjemplares()
        {
            try
            { return await _context.Ejemplares.Include(e => e.Libro).Where(e => e.EstadoEjemplar == "NO DISPONIBLE" || e.EstadoEjemplar == "EN PETICION" || e.EstadoEjemplar=="EN PRESTAMO").ToListAsync();
                
            }
            catch (System.Exception)
            {

                throw;
            }
        }



        public async Task<List<Ejemplar>> buscarEj()
        {
            var ejemplares = await obtenerEjemplares();
            return ejemplares;
        }
        public async Task<List<Usuario>> buscarUs()
        {
            return _context.Usuarios.Where(u=>u.Estado=="ACTIVO").ToList();
        }
        public async Task<List<Reserva>> ObtenerReservasPdf()
        {
            return _context.Reserva.Include(r => r.Ejemplar).ThenInclude(l => l.Libro).Include(r => r.Usuario)
                .ToList();
        }
        public async Task<List<Reserva>> ObtenerReservas()
        {
            return _context.Reserva.Include(r => r.Ejemplar).ThenInclude(l => l.Libro).Include(r => r.Usuario).Where(r => r.Estado == "ACTIVA" || r.Estado=="ACEPTADA")
                .ToList();
        }
        public async Task<ResponseModel> Crear(Reserva reserva, ClaimsPrincipal User)
        {
            Console.WriteLine("LLEGAMOS A CREAR....");
            int status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_reserva", _configuracionServices.ObtenerRolUserOnline(User));
            if (status == 200)
            {
                reserva.FechaReserva = DateTime.Now;
                reserva.Ejemplar = await _ejemplarservices.BuscarEjemplar(reserva.IdEjemplar);
                reserva.Usuario = await _usuariosServices.Buscar(reserva.IdUsuario);

                reserva.Estado = "ACTIVA";
                _context.Reserva.Add(reserva);
                _context.SaveChanges();

            }
            var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            return resultado;


        }

        public async Task<int> RegistrarMovil(Reserva reserva, int Id_rol)
        {
            Console.WriteLine("LLEGAMOS A CREAR....");
            int status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_reserva", Id_rol);
            if (status == 200)
            {

                reserva.FechaReserva = DateTime.Now;
                Usuario usuario = await _usuariosServices.Buscar(reserva.IdUsuario);
                Console.WriteLine($"este es el numero de documento del usuarionque va hacer la reserva {usuario.Numero_documento}");
                var ejemplar = await _ejemplarservices.BuscarEjemplar(reserva.IdEjemplar);

                if (ejemplar != null && usuario != null)
                {
                     reserva.Ejemplar = ejemplar;
                     reserva.Usuario = usuario;

                      reserva.Estado = "ACTIVA";
                    _context.Add(reserva);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("se ha creado la reserva");

                }else{
                    Console.WriteLine("no esta llegando algun dato");
                }

               
               

            }
            return status;
        }
        public async Task<bool> Buscar(int dato)
        {
            var buscar = await _context.Reserva.FirstOrDefaultAsync(p => p.IdEjemplar == dato && p.Estado == "ACTIVO");
            if (buscar != null)
            {
                 
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<Reserva> BuscarReservaExistente(int IdEjemplar)
        {
            var reserva = await _context.Reserva.Include(r => r.Usuario).Include(r => r.Ejemplar).ThenInclude(r => r.Libro).FirstOrDefaultAsync(p => p.IdEjemplar == IdEjemplar && p.Estado == "ACTIVO");
            if (reserva != null)
            {
                Console.WriteLine($"estamos encontrando una reserva asociada a: {reserva.Usuario.Name}");
                Console.WriteLine($"estamos encontrando una reserva asociada a: {reserva.Ejemplar.Libro.Nombre}");
                return reserva;
            }
            else
            {
                Console.WriteLine("vamos a devolver la reserva vacia");
                return null;
            }
        }
        public async Task<ResponseModel> Cambiarestado(int Id, ClaimsPrincipal User)
        {
            Console.WriteLine("LLEGANDO A CAMBIAR ESTADO");
            int status = _configuracionServices.ValidacionConfiguracionActiva("Rechazar_reserva", _configuracionServices.ObtenerRolUserOnline(User));
            if (status == 200)
            {
                var encontrar = await _context.Reserva.FirstOrDefaultAsync(e => e.Id == Id);
                if (encontrar != null)
                {
                    Console.WriteLine("Debería de desactivar");
                    encontrar.Estado = "RECHAZADA";
                    await _context.SaveChangesAsync();
                }
            }
            var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            return resultado;
        }
        public async Task<ResponseModel> CambiarestadoPeti(int id, ClaimsPrincipal User)
        {
            int status = _configuracionServices.ValidacionConfiguracionActiva("Aceptar_reserva", _configuracionServices.ObtenerRolUserOnline(User));
            if (status == 200)
            {
                var encontrar = await _context.Reserva.FirstOrDefaultAsync(e => e.Id == id);
                if (encontrar != null)
                {
                    encontrar.Estado = "ACEPTADA";
                    await _context.SaveChangesAsync();
                }
            }
            var resultado = _configuracionServices.MensajeRespuestaValidacionPermiso(status);
            return resultado;
        }

        public async Task<Reserva> enviarR(int id)
        {
            var reserva = _context.Reserva
                .Include(r => r.Ejemplar).ThenInclude(l => l.Libro)
                .Include(r => r.Usuario)
                .FirstOrDefault(u => u.Id == id);
            if (reserva == null)
            {
                Console.WriteLine("no esta encontrando la reserva");
                // Handle the case when the reservation is not found, maybe log or throw an exception
                return null; // or throw new Exception("Reservation not found")
            }
            return reserva;                                                      
        }

        public async Task<bool> Encontrarreserva(int idusuario)
        {
            Console.WriteLine("BUSCANDO RESERVA");
            var buscar = _context.Reserva.FirstOrDefault(p => p.IdUsuario == idusuario && p.Estado=="ACTIVO");
            if (buscar != null)
            {
                Console.WriteLine("RESERVA ENCONTRADA");
                return true;
            }
            else
            {
                Console.WriteLine("NO SE ENCONTRÓ NADA");
                return false;
            }
        }
        public async Task<List<Reserva>> Rechazadas()
        {
            return _context.Reserva.Include(r => r.Ejemplar).ThenInclude(l => l.Libro).Include(r => r.Usuario).Where(r => r.Estado == "RECHAZADA")
            .ToList();
        }
        public async Task<List<Reserva>> Buscar(string buscar)
        {
            int idEjemplar;
            bool esIdEjemplar = int.TryParse(buscar, out idEjemplar);
            return _context.Reserva.Where(r => (r.Estado == "ACTIVO" || r.Estado == "ACEPTADA") && r.Ejemplar.Libro.Nombre.Contains(buscar) || r.Usuario.Name.Contains(buscar) || r.Usuario.Apellido.Contains(buscar) || r.Ejemplar.Isbn_libro.Contains(buscar) || (esIdEjemplar && r.Id == idEjemplar)).ToList();
        }
        public async Task<List<Reserva>> BuscarR(int id)
        {
            Console.WriteLine("BUSCANDO PORQUE ES ROL 2");
            return _context.Reserva.Include(r => r.Ejemplar).ThenInclude(l => l.Libro).Include(r => r.Usuario).Where(r => r.Usuario.Id==id)
                .ToList();
        }
        public async Task<List<Reserva>> Buscarporfecha(DateTime? Fecha)
        {
            if (Fecha.HasValue)
            {
                Fecha = Fecha.Value.Date;
                return _context.Reserva
                    .Where(f => f.FechaReserva.Date == Fecha && (f.Estado == "ACTIVO" || f.Estado == "ACEPTADA"))
                    .Include(r => r.Ejemplar)
                    .ThenInclude(l => l.Libro)
                    .Include(r => r.Usuario)
                    .ToList();
            }
            else
            {
                // Si la fecha es nula, devuelve una lista vacía o maneja el caso según tus necesidades
                return new List<Reserva>();
            }
            //return _context.Reserva.Where(f=>f.FechaReserva == Fecha && f.Estado=="ACTIVO" || f.Estado=="ACEPTADA").Include(r => r.Ejemplar).ThenInclude(l => l.Libro).Include(r => r.Usuario).ToList();
        }
    }
}
