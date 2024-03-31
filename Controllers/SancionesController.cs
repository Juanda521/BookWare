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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace tallerbiblioteca.Controllers
{
    [Authorize]
    public class SancionesController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private readonly SancionesServices _sancionesServices;

        public SancionesController(BibliotecaDbContext context, SancionesServices sancionesServices)
        {
            _context = context;
            _sancionesServices = sancionesServices;
        }

        // GET: Sanciones
        // public async Task<IActionResult> Index()
        // {
        //     return View(await _sancionesServices.ObtenerSanciones());
        // }

        

        public async Task<IActionResult> Index(string busqueda, int pagina = 1, int itemsPagina = 4)
        {

            ViewBag.Devoluciones = new SelectList(await _sancionesServices.ObtenerDevoluciones(),"Id","Prestamo.Peticion.Usuario.Numero_documento") ;
            var sancion = await _sancionesServices.ObtenerSanciones();
            var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
            var idUsuarioOnline = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (rolUsuario != "1" && rolUsuario != "3")
            {
                Console.WriteLine("hay en linea un usuario");
                sancion = sancion.Where(p => p.Devolucion.Prestamo.Peticion.Usuario.Id.ToString() == idUsuarioOnline).ToList();
            }
            else
            {
                Console.WriteLine("hay en linea un administrador o un alfabetizador");
            }

            if (busqueda!=null)
            {
                busqueda = busqueda.ToLower();
                if (int.TryParse(busqueda, out int Id))
                {
                    sancion = sancion.Where(u => u.Motivo_sancion.ToLower().Contains(busqueda) ||  u.Devolucion.Prestamo.Peticion.Usuario.Name.ToLower().Contains(busqueda)  || u.Id.ToString().Contains(busqueda)).ToList();
                }
                else
                {
                    sancion = sancion.Where(u => u.Motivo_sancion.Contains(busqueda)).ToList();

                }
            }
            // Realiza la paginación
            int totalsanciones = sancion.Count;
            var sancionesPaginadas = sancion.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();
            // Crea el objeto de paginación
            Paginacion<Sancion> paginacion = new Paginacion<Sancion>(sancionesPaginadas, totalsanciones, pagina, itemsPagina);

            return View(paginacion);
        }

        // GET: Sanciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sanciones == null)
            {
                return NotFound();
            }

            var sancion = await _context.Sanciones
                .Include(s => s.Devolucion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sancion == null)
            {
                return NotFound();
            }

            return View(sancion);
        }

        // GET: Sanciones/Create
        public IActionResult Create()
        {
            ViewData["Id_devolucion"] = new SelectList(_context.Devoluciones, "Id", "Id");
            return View();
        }

        // POST: Sanciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Id_devolucion,Motivo_sancion,Fecha_Sancion")] Sancion sancion)
        {
            Console.WriteLine("hola desdde registrar devoluciones");
            Console.WriteLine($"id: {sancion.Id}");
            Console.WriteLine($"id: {sancion.Id_devolucion}");
            Console.WriteLine($"id{sancion.Motivo_sancion}");
            Console.WriteLine($"id{sancion.Fecha_Sancion}");
            int Status = await _sancionesServices.Registrar(sancion, User);


            MensajeRespuestaValidacionPermiso(Status);



            return RedirectToAction(nameof(Index));
        }

        private void MensajeRespuestaValidacionPermiso(int Status)
        {

            var resultado = new ResponseModel();

            if (Status == 200)
            {
                resultado.Mensaje = "La accion se ha realizado con exito";
                resultado.Icono = "success";
                // TempData["Mensaje"] = "La accion se ha realizado con exito";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (Status == 401)
            {  //si el permiso no lo puede realizar el usuario debido a que su rol no le permite realizar la accion ( status 401)
                resultado.Mensaje = "No tienes permiso para realizar esta accion";
                resultado.Icono = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (Status == 402)
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

        // GET: Sanciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sanciones == null)
            {
                return NotFound();
            }

            var sancion = await _context.Sanciones.FindAsync(id);
            if (sancion == null)
            {
                return NotFound();
            }
            ViewData["Id_devolucion"] = new SelectList(_context.Devoluciones, "Id", "Id", sancion.Id_devolucion);
            return View(sancion);
        }



         [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Editar(int Id)
        {
           
            var Sancion = await _sancionesServices.Buscar(Id);
            if (Sancion != null)
            {

                MensajeRespuestaValidacionPermiso(await _sancionesServices.Editar(Sancion, User));

            }
            else
            {
                Console.WriteLine("no se esta encontrando una devolucion  con el id:" + Id);
            }
            return RedirectToAction(nameof(Index));

        }
       

     

        // POST: Sanciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int Status = await _sancionesServices.Eliminar(id, User);


            MensajeRespuestaValidacionPermiso(Status);



            return RedirectToAction(nameof(Index));
        }

        private bool SancionExists(int id)
        {
          return (_context.Sanciones?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
