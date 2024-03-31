using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using System.Text;
using System.IO;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using System.Security.Claims;



namespace tallerbiblioteca.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly PrestamosServices _prestamosServices;
        private readonly DevolucionesServices _devolucionesServices;

        public DevolucionesController(BibliotecaDbContext context, DevolucionesServices devolucionesServices, PrestamosServices prestamosServices)
        {
            _context = context;
            _devolucionesServices = devolucionesServices;
            _prestamosServices = prestamosServices;
        }


        public async Task<IActionResult> Index(int? id, DateTime? fechaDevolucion, string busqueda, int pagina = 1, int itemsPagina = 10)
        {


            ViewBag.Id_prestamo = new SelectList(await _prestamosServices.ObtenerPrestamosEnCurso(), "Id", "Peticion.Usuario.Numero_documento");
            var devolucion = await _devolucionesServices.ObtenerDevoluciones();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);

            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (rol != "1" && rol != "3")
            {
                devolucion = devolucion.Where(d => d.Prestamo.Peticion.Usuario.Id.ToString() == idUsuario).ToList();
            }
            else
            {
                Console.WriteLine("Esta en administrado");
                devolucion = await _devolucionesServices.ObtenerDevoluciones();
            }



            if (busqueda != null || fechaDevolucion != null)
            {
                Console.WriteLine("vamos a buscar");
                devolucion = _devolucionesServices.BuscarDevolucion(busqueda, fechaDevolucion);
            }

            var sanciones = await _context.Sanciones.Include(s=>s.Devolucion).ThenInclude(d=>d.Prestamo).ThenInclude(p=>p.Peticion).ThenInclude(pt=>pt.Usuario).ToListAsync();

            int totalDevoluciones = devolucion.Count;
            int total = (totalDevoluciones / itemsPagina) + 1;
            var devolucionesPaginados = devolucion.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

            Paginacion<Devolucion> paginacion = new Paginacion<Devolucion>(devolucionesPaginados, total, pagina, itemsPagina);
            paginacion.Sanciones = sanciones;
            return View(paginacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDevoluciones()
        {
            // string Id = Request.Form["Id"];
            string Id_prestamo = Request.Form["Id_prestamo"];
            string Observaciones = Request.Form["Observaciones"];

            if (int.TryParse(Id_prestamo, out int idPrestamoInt))
            {
                Console.WriteLine("id del ejemplar a registrar: {0}", idPrestamoInt);
            }
            else
            {
                Console.WriteLine("no esta parseando el ejemplar");
                return RedirectToAction("Index", "Devoluciones");
            }


            //Verifica si ya existe una devolución con el mismo ID de préstamo
            var devolucionExistente = await _context.Devoluciones
                .AnyAsync(p => p.Id_prestamo == idPrestamoInt);

            var resultado = new ResponseModel();

            //Si la devolución ya existe, retorna un error
            if (devolucionExistente)
            {
                Console.WriteLine("Ya existe una devolución con el ID de préstamo proporcionado.");
                resultado.Mensaje = "Ya existe una devolución con el ID de préstamo proporcionado.";
                resultado.Icono = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                return RedirectToAction("Index", "Devoluciones");
            }

            Devolucion devolucion = new();
            devolucion.Id_prestamo = idPrestamoInt;
            devolucion.Observaciones = Observaciones;
            devolucion.Fecha_devolucion = DateTime.Now;

            // Continúa con la validación del modelo y la creación de la devolución
            if (ModelState.IsValid)
            {
                try
                {
                    // Intenta registrar la devolución
                    MensajeRespuestaValidacionPermiso(await _devolucionesServices.Registrar(devolucion, User));
                    return RedirectToAction("Index", "Devoluciones");
                }
                catch (Exception ex)
                {
                    resultado.Mensaje = "Hubo un error al intentar registrar la devolución: ";
                    resultado.Icono = "error";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    return RedirectToAction("Index", "Devoluciones");
                }
            }
            else
            {
                resultado.Mensaje = "Hubo un error al intentar registrar la devolución desde el modelo actualizar: ";
                resultado.Icono = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                return RedirectToAction("Index", "Devoluciones");
            }
        }



        private void MensajeRespuestaDevolucion(int status)
        {
            Console.WriteLine(status);
            var resultado = new ResponseModel();
            switch (status)
            {
                case 200:
                    resultado.Mensaje = "La accion se ha realizado con exito";
                    resultado.Icono = "success";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 401:
                    resultado.Mensaje = "El permiso para realizar esta accion no se encuentra activo";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 402:
                    resultado.Mensaje = "El permiso para realizar esta accion no se encuentra activo";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 500:
                    resultado.Mensaje = "ya se ha registrado la devolucion de este prestamo";
                    resultado.Icono = "error";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                default:
                    Console.WriteLine("i'm failing in the name of permission");
                    break;
            }

        }

        private void MensajeRespuestaValidacionPermiso(int status)
        {

            var resultado = new ResponseModel();

            if (status == 200)
            {
                resultado.Mensaje = "La accion se ha realizado con exito";
                resultado.Icono = "success";
                // TempData["Mensaje"] = "La accion se ha realizado con exito";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (status == 401)
            {  //si el permiso no lo puede realizar el usuario debido a que su rol no le permite realizar la accion ( status 401)
                resultado.Mensaje = "No tienes permiso para realizar esta accion";
                resultado.Icono = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (status == 402)
            {
                resultado.Mensaje = "El permiso para realizar esta accion no se encuentra activo";
                resultado.Icono = "info";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else
            {
                Console.WriteLine("i'm failing in the name of permission");
            }
            //return (string)TempData["Mensaje"];
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int Id,string texto)
        {
            Console.WriteLine(texto);
            var Devolucion = await _devolucionesServices.Buscar(Id);
            if (Devolucion != null)
            {

                MensajeRespuestaValidacionPermiso(await _devolucionesServices.Editar(Devolucion, User,texto));

            }
            else
            {
                Console.WriteLine("no se esta encontrando una devolucion  con el id:" + Id);
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar()
        {
            string id_prestamo = Request.Form["id_prestamo"];
            string observacion = Request.Form["observaciones"];
            Console.WriteLine("aca deberia copier el id del prestamo: {0} ", id_prestamo);
            Console.WriteLine("aca deberia copiar la observacion que se le ha hecho al libro: {0} ", observacion);

            Devolucion devolucion = new()
            {
                Observaciones = observacion,
                Fecha_devolucion = _devolucionesServices.obtenerFechaActual()
            };
            Console.WriteLine(devolucion.Fecha_devolucion);


            if (int.TryParse(id_prestamo, out int idPrestamoInt))
            {
                devolucion.Id_prestamo = idPrestamoInt;
                Console.WriteLine("id del prestamo a registrar: {0}", idPrestamoInt);
                Console.WriteLine("vamos a validar la devolcion existente con el prestamo");
                var devolucionExistente = await _devolucionesServices.BuscarDevolucionExistente(idPrestamoInt);

                if (devolucionExistente)
                {
                    Console.WriteLine("ya el prestamo se ha devuelto");
                    MensajeRespuestaDevolucion(500);
                    return RedirectToAction("Index", "Prestamos");
                }
                else
                {
                    Console.WriteLine("no se encontraron devoluciones con ese prestamo");
                }
            }

            MensajeRespuestaValidacionPermiso(await _devolucionesServices.Registrar(devolucion, User));
            return RedirectToAction(nameof(Index));
        }




        // GET: Devoluciones/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Devoluciones == null)
            {
                return NotFound();
            }

            var devolucion = await _context.Devoluciones
                .Include(d => d.Prestamo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (devolucion == null)
            {
                return NotFound();
            }

            return View(devolucion);
        }

        // POST: Devoluciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            int status = await _devolucionesServices.Eliminar(id, User);

            MensajeRespuestaValidacionPermiso(status);

            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionExists(int id)
        {
            return (_context.Devoluciones?.Any(e => e.Id == id)).GetValueOrDefault();
        }



        // [HttpGet]
        // public IActionResult GenerarReporteDevolucionesPDF()
        // {
        //     // Obtiene los datos de las devoluciones
        //     var devoluciones = _context.Devoluciones.ToList(); // Asegúrate de que _context esté configurado correctamente

        //     // Crear un nuevo documento PDF con QuestPDF
        //     var document = new DocumentTemplate();

        //     // Agrega contenido al documento
        //     foreach (var devolucion in devoluciones)
        //     {
        //         document.AddSection(section =>
        //         {
        //             section.Header().Text($"Devolución ID: {devolucion.Id}", TextStyle.Default.Size(14).Bold());
        //             section.Content().Row().Cell().Text($"ID Préstamo: {devolucion.IdPrestamo}");
        //             section.Content().Row().Cell().Text($"Observaciones: {devolucion.Observaciones}");
        //             section.Content().Row().Cell().Text($"Fecha de Devolución: {devolucion.FechaDevolucion.ToString("dd/MM/yyyy")}");
        //         });
        //     }

        //     // Genera el PDF
        //     var pdfBytes = document.GeneratePdf();

        //     // Devuelve el PDF como un archivo
        //     return File(pdfBytes, "application/pdf", $"devoluciones_{DateTime.Now.Ticks}.pdf");
        // }






    }
}