using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
using Newtonsoft.Json;
using tallerbiblioteca.Services;

namespace tallerbiblioteca.Controllers
{
    public class GenerosController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly GenerosServices _generosServices; 

        public GenerosController(BibliotecaDbContext context,GenerosServices generosServices)
        {
            _context = context;
            _generosServices =  generosServices;
        }

        // GET: Generoes
        public async Task<IActionResult> Index(string busqueda,int itemsPagina = 10, int pagina = 1)
        {
            var generos = await _generosServices.ObtenerGeneros();

            //si viene algo en el parametro busqueda procederemos a añadir a la lista de libros a mostrar los libros que coincidan con la busqueda
            if (busqueda != null)
            {
                busqueda.ToLower();
                generos = _generosServices.busqueda(busqueda);
            }
            
            int totalGeneros = generos.Count;
            int total = (totalGeneros/ itemsPagina) + 1;
            var generosPaginados = generos.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

            Paginacion<Genero> paginacion = new Paginacion<Genero>(generosPaginados, total, pagina, itemsPagina);
            return View(paginacion);
        }

         [HttpGet]
        public async Task<IActionResult>GetGeneros(){
            var generos = await _context.Genero.ToListAsync();
            return Json(generos);
        }

        public IActionResult Desactivar(int id)
        {
            MensajeRespuestaValidacionPermiso(_generosServices.Desactivar(id, User));
          
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Activar(int id)
        {
            MensajeRespuestaValidacionPermiso(_generosServices.Activar(id, User));
          
            return RedirectToAction(nameof(Index));

        }

        // GET: Generoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Genero == null)
            {
                return NotFound();
            }

            var genero = await _context.Genero
                .FirstOrDefaultAsync(m => m.Id == id);

            var librosRelacionados = await _context.GenerosLibros.Where(a=>a.Id_genero  == id).ToListAsync();
            int numeroLibros = librosRelacionados.Count;
            Console.WriteLine(numeroLibros);
            if (librosRelacionados==null){
                Console.WriteLine("no esta encontrando ");
            }
            var librosMandar = new List<Libro>();
            foreach (var item in librosRelacionados)
            {
                Console.WriteLine("esta llegando aca");
                var libro = await _context.Libros.FirstOrDefaultAsync(l => l.Id == item.Id_libro);

                if (libro != null)
                {
                    librosMandar.Add(libro);
                    Console.WriteLine($"Este es el nombre del libro: {libro.Nombre}");
                };
            };
            genero.Libros  = librosMandar;
            
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        // GET: Generoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Generoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("Id,NombreGenero")] Genero genero)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         if (_context.Libros.FirstOrDefault(g => g.Id == genero.Id) == null)
        //         {
        //               _context.Add(genero);
        //             if(await _context.SaveChangesAsync()>0){
        //                   await _context.SaveChangesAsync();
        //                 TempData["Creacion"] = "Genero creado correctamente";
        //                 return RedirectToAction(nameof(Index));
        //             }
        //             else
        //             {
        //                 TempData["ErrorMessage"] = "Error al momento de la creacion";
        //             }
        //             ViewData["Id"] = new SelectList(_context.Libros, "Id", "Nombre");
        //             return RedirectToAction(nameof(Index));
        //         }
        //         else
        //         {
        //             TempData["ErrorMessage"] = "Error al momento de la creacion";
        //             return View();
        //         }

        //     }
        //     else
        //     {
        //         TempData["ErrorMessage"] = "Error";
        //         return View();
        //     }
        //     //if (ModelState.IsValid)
        //     //{
        //     //    _context.Add(genero);
        //     //    await _context.SaveChangesAsync();
        //     //    return RedirectToAction(nameof(Index));
        //     //}
        //     //return View(genero);
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreGenero")] Genero genero)
        {
            int status = await _generosServices.Registrar(genero, User);

            MensajeRespuestaValidacionPermiso(status);

            return RedirectToAction("Index","Libros");
        }

    
            
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>CreateGenero(Genero genero){

            string nombreGenero= Request.Form["NombreGenero"];
            Console.WriteLine(nombreGenero);
            
            var generoExistente = await _context.Genero
                    .FirstOrDefaultAsync(g => g.NombreGenero.ToLower() == genero.NombreGenero.ToLower());

                    var resultado = new ResponseModel();

                if (generoExistente != null)
                {
                    Console.WriteLine("Ya existe este genero");
                    resultado.Mensaje = "Ya existe este género.";
                    resultado.Icono = "error";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    return RedirectToAction("Index", "Libros");
                }
                Console.WriteLine("Con el modelo validado");
                Console.WriteLine($"ya va empezar a realizar los servicios");
                MensajeRespuestaValidacionPermiso(await _generosServices.Registrar(genero, User));
                return RedirectToAction("Index", "Libros");
            
            
        }
            
        

    


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>UpdateGenero(Genero genero)
        {

            if (ModelState.IsValid)
            {
                Console.WriteLine("Con el modelo validado");
                 Console.WriteLine($"ya va empezar a realizar los servicios");
                MensajeRespuestaValidacionPermiso( await _generosServices.Editar(genero,User));
                return RedirectToAction("Index","Generos");
            }
            else
            {
                Console.WriteLine("El modelo no es válido");
                // Devuelve la vista con el modelo para que pueda mostrar los errores de validación
                 return RedirectToAction("Index", "Generos");
            }

        }



        // GET: Generoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Genero == null)
            {
                return NotFound();
            }

            var genero = await _context.Genero.FindAsync(id);
            if (genero == null)
            {
                return NotFound();
            }
            return View(genero);
        }

        // POST: Generoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreGenero")] Genero genero)
        {
            int status = await _generosServices.Editar(genero, User);

            MensajeRespuestaValidacionPermiso(status);

            return RedirectToAction(nameof(Index));
        }

        // GET: Generoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Genero == null)
            {
                return NotFound();
            }

            var genero = await _context.Genero
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        // POST: Generoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Genero == null)
            {
                return Problem("Entity set 'BibliotecaDbContext.Genero'  is null.");
            }
            var genero = await _context.Genero.FindAsync(id);
            if (genero != null)
            {
                _context.Genero.Remove(genero);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeneroExists(int id)
        {
          return (_context.Genero?.Any(e => e.Id == id)).GetValueOrDefault();
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
    }
}
