using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;


namespace tallerbiblioteca.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {

        private readonly BibliotecaDbContext _context;
        private readonly ReservasServices _ReservasServices;


        public ReservasController(BibliotecaDbContext context, ReservasServices reservasServices)
        {
            _context = context;
            _ReservasServices = reservasServices;
        }
        private void Alerta(int status)
        {
            var resultado = new ResponseModel();

            if (status == 202)
            {
                resultado.Mensaje = "Ya tienes una reserva pendiente";
                resultado.Icono = "error";
                // TempData["Mensaje"] = "La accion se ha realizado con exito";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }else if(status==203){
                resultado.Mensaje = "Este usuario ya  tiene una reserva pendiente";
                resultado.Icono = "error";
                // TempData["Mensaje"] = "La accion se ha realizado con exito";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else
            {
                Console.WriteLine("i'm failing in the name of permission");
            }
            //return (string)TempData["Mensaje"];
        }
        private void MensajeRespuestaValidacionPermiso(ResponseModel resultado)
        {

            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
        }

        // GET: Reserva
        public async Task<IActionResult> Index(string? buscar, DateTime? fecha, int pagina = 1, int itemsPagina = 5, int? id = null)
        {
            try
            {
                var reservas = await _ReservasServices.ObtenerReservas();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                Console.WriteLine("estamos aca");
                if (User.IsInRole("2") && userIdClaim != null)
                {
                    int userId = Convert.ToInt32(userIdClaim.Value);
                    reservas = await _ReservasServices.BuscarR(userId);
                }
                if (buscar != null)
                {
                    reservas = await _ReservasServices.Buscar(buscar);
                }
                else if (fecha.HasValue)
                {
                    reservas = await _ReservasServices.Buscarporfecha(fecha.Value.Date);
                }
                reservas = reservas.OrderBy(p => p.Estado == "ACTIVO" ? 0 : 1).ToList();
                int totalItems = reservas.Count;
                int totalPaginas = (int)Math.Ceiling((double)totalItems / itemsPagina);

                int indiceInicio = (pagina - 1) * itemsPagina;

                var reservacionesPaginadas = reservas
                    .Skip(indiceInicio)
                .Take(itemsPagina)
                .ToList();
                var ejemplares = await _ReservasServices.obtenerEjemplares();
                var usuarios = await _ReservasServices.buscarUs();

                var ejemplaresSelect = new SelectList(ejemplares, "Id", "Libro.Nombre");
                var usuariosSelectList = new SelectList(usuarios, "Id", "Name");
                ViewBag.Usuarios = usuariosSelectList;
                ViewBag.ejemplares = ejemplaresSelect;
                var paginacion = new ReservaModel(new Paginacion<Reserva>(reservacionesPaginadas, totalPaginas, pagina, itemsPagina), new Reserva());

                return View(paginacion);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                var ejemplares = await _ReservasServices.obtenerEjemplares();
                var usuarios = await _ReservasServices.buscarUs();

                var ejemplaresSelect = new SelectList(ejemplares, "Id", "Libro.Nombre");
                var usuariosSelectList = new SelectList(usuarios, "Id", "Name");
                ViewBag.Usuarios = usuariosSelectList;
                ViewBag.ejemplares = ejemplaresSelect;

                return View();
            }catch(Exception) { return RedirectToAction("Error"); }
        }


        // POST: Reserva/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            try
            {
                bool buscreserva = await _ReservasServices.Encontrarreserva(reserva.IdUsuario);
                if (buscreserva)
                {
                    Console.WriteLine("RESERVA ENCONTRADA, DEBERÍA ABRIR");
                    Alerta(202);
                    //TempData["ReservaPendiente"] = "True";
                    return RedirectToAction("Catalog", "Libros");
                }
                else
                {
                    MensajeRespuestaValidacionPermiso(await _ReservasServices.Crear(reserva, User));
                }
                return RedirectToAction(nameof(Index));
            }catch(Exception) { return RedirectToAction("Error"); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createad(Reserva reserva)
        {
            try
            {
                bool buscreserva = await _ReservasServices.Encontrarreserva(reserva.IdUsuario);
                if (buscreserva)
                {
                    Console.WriteLine("RESERVA ENCONTRADA, DEBERÍA ABRIR");
                    Alerta(203);
                    //TempData["ReservaPendiente"] = "True";
                    //return RedirectToAction("Catalog", "Libros");
                }
                else
                {
                    MensajeRespuestaValidacionPermiso(await _ReservasServices.Crear(reserva, User));
                }
                return RedirectToAction(nameof(Index));
            }catch (Exception) { return RedirectToAction("Error"); }
        }

        // GET: Reserva/Delete/5
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var reserva = await _ReservasServices.enviarR(id);
        //    return View(reserva);
        //}

        // POST: Reserva/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                Console.WriteLine("el id que acaba de llegar es ", Id);
                //var Eliminar = await _ReservasServices.Cambiarestado(Id, User);
                MensajeRespuestaValidacionPermiso(await _ReservasServices.Cambiarestado(Id, User));
                return RedirectToAction(nameof(Index));
            }catch (Exception){ return RedirectToAction("Error"); }
        }
        public async Task<IActionResult> Rechazadas(int pagina = 1, int itemsPagina = 5)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                var eliminadas = await _ReservasServices.Rechazadas();
                int totalItems = eliminadas.Count;
                int totalPaginas = (int)Math.Ceiling((double)totalItems / itemsPagina);

                int indiceInicio = (pagina - 1) * itemsPagina;

                var reservacionesPaginadas = eliminadas
                    .Skip(indiceInicio)
                    .Take(itemsPagina)
                    .ToList();
                var paginacion = new Paginacion<Reserva>(reservacionesPaginadas, totalPaginas, pagina, itemsPagina);

                return View(paginacion);
            }catch (Exception)
            {
                return RedirectToAction("Error");
            } 

        }
    }
}
