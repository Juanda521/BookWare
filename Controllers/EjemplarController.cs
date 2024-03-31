using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;

//uksl uqxi npnx lyde
namespace tallerbiblioteca.Controllers
{
     [Authorize] //anotacion para especificar que debe estar aunthenticado ya el usuario (logeado)
    public class EjemplarController : Controller
    {
        private readonly EjemplarServices _ejemplarServices;

          private LibrosServices _librosServices;

        public EjemplarController(EjemplarServices ejemplarServices,LibrosServices librosServices)
        {
            _ejemplarServices = ejemplarServices;
            _librosServices = librosServices;
        }

        public async Task<IActionResult> Index(string busqueda, int pagina = 1, int itemsPagina = 6)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                var ejemplares = await _ejemplarServices.ObtenerEjemplares();
                if (busqueda != null)
                {
                    busqueda = busqueda.ToLower();
                    ejemplares = ejemplares.Where(e => e.Id.ToString().Contains(busqueda) || e.Libro.Nombre.ToLower().Contains(busqueda) || e.Isbn_libro.ToString().Contains(busqueda) || e.EstadoEjemplar.ToString().Contains(busqueda)).ToList();
                }

                int totalEjemplares = ejemplares.Count;
                int total = (totalEjemplares / itemsPagina) + 1;
                var ejemplaresPaginados = ejemplares.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();
                Paginacion<Ejemplar> paginacion = new(ejemplaresPaginados, total, pagina, itemsPagina);
                ViewData["libros"] = new SelectList(await _librosServices.ObtenerLibros(), "Id", "Nombre");
                ViewData["libros"] = new SelectList(await _librosServices.ObtenerLibros(), "Id", "Nombre");
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
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                ViewData["libros"] = new SelectList(await _librosServices.ObtenerLibros(), "Id", "Nombre");
                return View();
            }catch(Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Id_libro,Isbn_libro,EstadoEjemplar")] Ejemplar ejemplar)
        {
            try
            {
                Console.WriteLine("aca debe escribir el ejemplar");
                Console.WriteLine($"esta llegando esto desde el formulario estado {ejemplar.EstadoEjemplar}");
                MensajeRespuestaValidacionPermiso(await _ejemplarServices.Registrar(ejemplar, User));
                return RedirectToAction("Index", "Libros");
            }catch(Exception)
            {
                return RedirectToAction("Error");
            }
        } 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>CreateFromLibros(){
            try
            {
                Console.WriteLine("hablalo desde registrar ejempla desde la vista de libros");
                string id_libro = Request.Form["Id_libro"];
                string isbn_ejemplar = Request.Form["Isbn_ejemplar"];
                Console.WriteLine("aca deberia copier el id del libro: {0} ", id_libro);
                Console.WriteLine("aca deberia copier el isbn del libro: {0} ", isbn_ejemplar);

                if (int.TryParse(id_libro, out int idLibroInt))
                {
                    Console.WriteLine("id del ejemplar a registrar: {0}", idLibroInt);
                }
                else
                {
                    Console.WriteLine("no esta parseando el ejemplar");
                    return RedirectToAction("Index", "Libros");
                }

                Ejemplar ejemplar = new();
                ejemplar.Id_libro = idLibroInt;
                ejemplar.Isbn_libro = isbn_ejemplar;
                ejemplar.EstadoEjemplar = "DISPONIBLE";

                Console.WriteLine($"ya va empezar a realizar los servicios");
                MensajeRespuestaValidacionPermiso(await _ejemplarServices.Registrar(ejemplar, User));
                return RedirectToAction("Index", "Libros");
            }catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                var ejemplar = await _ejemplarServices.BuscarEjemplar(Id);
                if (ejemplar != null)
                {

                    MensajeRespuestaValidacionPermiso(await _ejemplarServices.Edit(User, ejemplar));
                }
                else
                {
                    Console.WriteLine("no se esta encontrando ejemplar con el id:" + Id);
                }
                return RedirectToAction("Index","Libros");
            }catch(Exception) {
                return RedirectToAction("Error");
            }
       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int Id){
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                Console.WriteLine("id:liminar", Id);
                MensajeRespuestaValidacionPermiso(await _ejemplarServices.Delete(Id, User));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error"); 
            }
            
        }
         private void MensajeRespuestaValidacionPermiso(int status)         
        {
            Console.WriteLine(status);
           var resultado = new ResponseModel();

            if (status == 200)
            {       
                resultado.Mensaje  =  "La accion se ha realizado con exito";
                resultado.Icono  = "success";
                // TempData["Mensaje"] = "La accion se ha realizado con exito";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            } 
             else if (status == 203)
            {  //si el permiso no lo puede realizar el usuario debido a que su rol no le permite realizar la accion ( status 401)
                resultado.Mensaje  =  "Ya existe un ejemplar con este Isbn";
                resultado.Icono  = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (status == 401)
            {  //si el permiso no lo puede realizar el usuario debido a que su rol no le permite realizar la accion ( status 401)
                resultado.Mensaje  =  "No tienes permiso para realizar esta accion";
                resultado.Icono  = "error";
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            }
            else if (status == 402)
            {
                resultado.Mensaje  = "El permiso para realizar esta accion no se encuentra activo";
                resultado.Icono  = "info";
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
