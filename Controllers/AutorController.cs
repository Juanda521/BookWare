using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tallerbiblioteca.Context;
using tallerbiblioteca.Migrations;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;

namespace tallerbiblioteca.Controllers
{
    [Authorize(Roles = "1,3")]
    public class AutorController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly AutoresServices _autoresServices;

        public AutorController(BibliotecaDbContext context,AutoresServices autoresServices)
        {
            _context = context;
            _autoresServices = autoresServices;

        }


        public async Task<IActionResult>Index(string[] librosSeleccionados, string busqueda, int pagina =  1, int itemsPagina =  10)
        {
            var autores = await _autoresServices.ObtenerAutores();
            var libros = await _autoresServices.ObtenerLibros();

            // Aplica la búsqueda si es necesario
            if (!string.IsNullOrEmpty(busqueda))
            {
                autores = _autoresServices.BuscarAutores(busqueda);
            }

            

            int totalAutores = autores.Count;
            int total = (totalAutores/ itemsPagina) + 1;
            var autoresPaginados = autores.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

            Paginacion<Autor> paginacion = new Paginacion<Autor>(autoresPaginados, total, pagina, itemsPagina);
            return View(paginacion);
        }
        [HttpGet]
        public async Task<IActionResult>GetAutores(){
            var autores = await _context.Autores.ToListAsync();
            return Json(autores);
        }

        

        public IActionResult Desactivar(int id)
        {
            MensajeRespuestaValidacionPermiso(_autoresServices.Desactivar(id, User));
          
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Activar(int id)
        {
            MensajeRespuestaValidacionPermiso(_autoresServices.Activar(id, User));
          
            return RedirectToAction(nameof(Index));

        }


    

         public List<Libro> LibrosRelacionadosPorAutoro(int idAutor)
        {
            var librosDelAutorActual = _context.AutoresLibros
                .Where(al => al.Id_autor == idAutor)
                .Select(al => al.Id_libro)
                .ToList();

            var librosRelacionados = _context.AutoresLibros
                .Where(gl => librosDelAutorActual.Contains(gl.Id_libro) && gl.Id_autor != idAutor)
                .Select(gl => gl.Libro)
                .Distinct()
                .ToList();
                return librosRelacionados;
        }

        


        // GET: Autor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Autores == null)
            {
                return NotFound();
            }

            var autor = await _context.Autores
                .FirstOrDefaultAsync(m => m.Id == id);
                Console.WriteLine("vamos a buscar los libros");
              

            
            var librosRelacionados = await _context.AutoresLibros.Where(a=>a.Id_autor  == id).ToListAsync();
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
            
            if (autor == null)
            {
                return NotFound();
            }
            autor.Libros  = librosMandar;

            foreach (var item in autor.Libros)
            {
                Console.WriteLine($"nombre del libro {item.Nombre}");
            };


          

            return View(autor);
        }

        // GET: Autor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Autor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreAutor")] Autor autor)
        {
            if(ModelState.IsValid)
            {
                Console.WriteLine("Si es valido el modelo");
                int status = await _autoresServices.Registrar(autor, User);

                MensajeRespuestaValidacionPermiso(status);

                return RedirectToAction("Index","Libros");
            }else
            {
                Console.WriteLine("No esta validando");
                return RedirectToAction("Index","Libros");
            }
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>CreateAutor(Autor autor){
            
            if (ModelState.IsValid)
            {
                Console.WriteLine("Con el modelo validado");
                Console.WriteLine("Ya va empezar a realizar los servicios");
                MensajeRespuestaValidacionPermiso(await _autoresServices.Registrar(autor, User));
                return RedirectToAction("Index", "Libros");
            }
            else
            {
                Console.WriteLine("El modelo no es válido");
                // Devuelve la vista con el modelo para que pueda mostrar los errores de validación
                 return RedirectToAction("Index", "Libros");
            }
            
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>UpdateAutor(Autor autor)
        {

            if (ModelState.IsValid)
            {
                Console.WriteLine("Con el modelo validado de actualizar");
                Console.WriteLine($"ya va empezar a realizar los servicios");
                MensajeRespuestaValidacionPermiso( await _autoresServices.Editar(autor,User));
                return RedirectToAction("Index","Autor");
            }
            else
            {
                Console.WriteLine("El modelo no es válido");
                // Devuelve la vista con el modelo para que pueda mostrar los errores de validación
                 return RedirectToAction("Index", "Autor");
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

        // GET: Autor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Autores == null)
            {
                return NotFound();
            }

            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }
            return View(autor);
        }

        // POST: Autor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreAutor")] Autor autor)
        {
            int status = await _autoresServices.Editar(autor, User);

            MensajeRespuestaValidacionPermiso(status);

            return RedirectToAction(nameof(Index));
        }

        // GET: Autor/Delete/5
        // public async Task<IActionResult> Delete(int? id)
        // {
        //     if (id == null || _context.Autores == null)
        //     {
        //         return NotFound();
        //     }

        //     var autor = await _context.Autores
        //         .FirstOrDefaultAsync(m => m.Id == id);
        //     if (autor == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(autor);
        // }

        // // POST: Autor/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(int id)
        // {
        //     int status = await _autoresServices.Eliminar(id, User);

        //     MensajeRespuestaValidacionPermiso(status);

        //     return RedirectToAction(nameof(Index));
        // }

        // private bool AutorExists(int id)
        // {
        //   return (_context.Autores?.Any(e => e.Id == id)).GetValueOrDefault();
        // }
    }
}
