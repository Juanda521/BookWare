using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using Microsoft.AspNetCore.Authorization;

namespace tallerbiblioteca.Controllers
{
    [Authorize]
    // [RoleAuthorize("1")]
    public class PrestamosController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly PrestamosServices _prestamosServices;
        

        public PrestamosController(BibliotecaDbContext context,PrestamosServices prestamosServices)
        {
            _context = context;
            _prestamosServices = prestamosServices;
            
        }

        // GET: Prestamos
       // GET: Prestamos
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin,string busqueda,int Numero_pagina = 1, int itemsPagina = 10, int? id = null)
        {
            
            try
            {

                var prestamos = await _prestamosServices.ObtenerPrestamos();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                Console.WriteLine($"el rol del usuario en linea es: {rolUsuario}");

                var idUsuarioOnline  = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                Console.WriteLine($"el documento del usuario en linea es: {idUsuarioOnline}");

                if (rolUsuario!="1" && rolUsuario!="3"){
                    Console.WriteLine("hay en linea un usuario");
                    prestamos =  prestamos.Where(p=>p.Peticion.Usuario.Id.ToString()==idUsuarioOnline).ToList();
                }else{
                    Console.WriteLine("hay en linea un administrador o un alfabetizador");
                }

                if (busqueda!=null || fechaInicio!=null || fechaFin !=null){
                    Console.WriteLine("vamos a buscar");
                    prestamos =  _prestamosServices.BuscarPrestamos(busqueda,fechaInicio,fechaFin);
                }

                var PrestamosPaginados = prestamos.OrderBy(p => p.Estado == "EN CURSO" ? 0 : 1).Skip((Numero_pagina - 1) * itemsPagina).Take(itemsPagina).ToList();
                
                
                int total_prestamos  = prestamos.Count(); 
                int total = (total_prestamos/itemsPagina)+1;

                Paginacion<Prestamo> paginacion  =new Paginacion<Prestamo>(PrestamosPaginados,total,Numero_pagina,itemsPagina);

                return View(paginacion);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        
        public async Task<IActionResult> Calendario()
        {

            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                ViewBag.CantidadPrestamos = await _prestamosServices.TotalPrestamos();

                // Obtener la lista de devoluciones de forma asincrónica y contarlas
                List<Devolucion> devoluciones = await _context.Devoluciones.ToListAsync();
                ViewBag.CantidadDevoluciones = devoluciones.Count;

                // Obtener la lista de sanciones de forma asincrónica y contarlas
                List<Sancion> sanciones = await _context.Sanciones.ToListAsync();
                ViewBag.CantidadSanciones = sanciones.Count;

                return View();
            }catch(Exception )
            {
                return RedirectToAction("Error");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/Calendario")]
        public async Task<IActionResult> Prestamos(){
            try
            {
                var prestamos  = await _prestamosServices.ObtenerPrestamos();
                return Json(prestamos);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error en la función Prestamos: {ex.Message}");
                return BadRequest(); // Retornar un código de error 400
            }
        }
        
        [AllowAnonymous]
        [HttpGet]
        [Route("/api/Graficas")]
        public async Task<IActionResult> PrestamosGrafica(){
           try
            {
                var prestamos  = await _prestamosServices.ObtenerPrestamos();

                var prestamosPorLibro = prestamos
                    .GroupBy(p => p.Peticion.Ejemplar.Libro.Nombre)
                    .Select(g => new { Libro = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(g => g.Cantidad) // Ordenar por la cantidad de préstamos, de mayor a menor
            .Take(3) // Tomar solo los primeros 3 resultados
                    .ToList();

                // Construir la lista en el formato deseado
                var resultado = prestamosPorLibro
                    .Select(p => new { Libro = p.Libro, Cantidad = p.Cantidad })
                    .ToList();
                return Json(resultado);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error en la función PrestamosGrafica: {ex.Message}");
                return BadRequest(); // Retornar un código de error 400
            }
        }
        //GET: Prestamos/Create
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



                var peticiones = await _prestamosServices.ObtenerPeticiones();

                ViewBag.PeticionesCount = peticiones.Count;

                foreach (var item in peticiones)
                {
                    Console.WriteLine($"id peticion: {item.Id}");
                }
                ViewData["Peticion"] = new SelectList(peticiones, "Id", "Usuario.Name");
                ViewData["Id_usuario"] = new SelectList(_context.Usuarios, "Id", "Name");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
            
        }
        public async Task<IActionResult> Created(int id)
        {
            try
            {
                Console.WriteLine(id);
                Prestamo prestamo = new Prestamo
                {
                    Id_peticion = id // este sera el id de la peticion la cual estamos aceptando
                };

                var peticion = await _prestamosServices.BuscarPeticion(id);
                if (peticion == null)
                {
                    Console.WriteLine("No se encontró la petición");
                    return NotFound(); // Retorna un código de error 404
                }

                switch (peticion.Estado)
                {
                    case "ACEPTADA":
                        Console.WriteLine("La petición ya ha sido aceptada");
                        return RedirectToAction("Index", "Peticiones");
                    case "RECHAZADA":
                        Console.WriteLine("La petición ya ha sido rechazada");
                        return RedirectToAction("Rechazadas", "Peticiones");
                }

                MensajeRespuestaValidacionPermiso(await _prestamosServices.Registrar(prestamo, User));
                return RedirectToAction("Index", "Peticiones");
            }
            catch (Exception )
            {
                return RedirectToAction("Error");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Id_peticion,Fecha_inicio,Fecha_fin,Estado")] Prestamo prestamo)
        {
            try
            {
                Console.WriteLine("estamos en create");
                Console.WriteLine(prestamo.Fecha_inicio);
                var resultado = await _prestamosServices.Registrar(prestamo, User);
                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
            
        }

        private void MensajeRespuestaValidacionPermiso(ResponseModel resultado){
            
            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            
        }   
       // GET: Prestamos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
            if (rolUsuario == "2")
            {
                Console.WriteLine("El rol del usuario es " + rolUsuario);
                return View("Error");
            }
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var prestamo = await _prestamosServices.ObtenerPrestamo(id);
                if (prestamo == null)
                {
                    return NotFound();
                }
                
                return View(prestamo);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Renovar()
        {
            try
            {
                Console.WriteLine("llegamos a renovar ");
                string Id = Request.Form["Id"];
                // string Id_peticion = Request.Form["Id_peticion"];
                // string Fecha_inicio = Request.Form["Fecha_inicio"];
                string Fecha_fin = Request.Form["Fecha_fin"];

                if (int.TryParse(Id, out int idInt))
                {
                    var prestamo = await _prestamosServices.Buscar(idInt);
                    if (idInt != prestamo.Id)
                    {
                        return NotFound();
                    }
                    Console.WriteLine("tenemos el mismo prestamo");

                    if (DateTime.TryParse(Fecha_fin, out DateTime Fecha_finDate))
                    {
                        Console.WriteLine(Fecha_finDate);
                        // prestamo.Fecha_fin = Fecha_finDate;
                    }

                    ResponseModel resultado = new ResponseModel();

                    // Buscar el prestamo actual al que pertenece el prestamo a editar
                    var PrestamoExistente = await _prestamosServices.Buscar(prestamo.Id);
                    Console.WriteLine(PrestamoExistente.Fecha_fin);
                    if (PrestamoExistente != null)
                    {
                        if (PrestamoExistente.Estado == "FINALIZADO")
                        {
                            resultado.Mensaje = "No puedes renovar un préstamo que ya ha sido finalizado";
                            resultado.Icono = "error";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));

                        }

                        Console.WriteLine("El prestamo está en curso");

                        if (PrestamoExistente.Fecha_fin >= Fecha_finDate)
                        {
                            resultado.Mensaje = "No puedes poner la fecha final del préstamo antes de la actual registrada.";
                            resultado.Icono = "error";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        else if ((Fecha_finDate - PrestamoExistente.Fecha_fin).TotalDays > 15)
                        {
                            resultado.Mensaje = "La fecha final del préstamo no puede exceder  15 días de la fecha final del préstamo existente.";
                            resultado.Icono = "error";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        if (await _prestamosServices.ValidarReservaExistente(PrestamoExistente.Peticion.Ejemplar.Id))
                        {
                            Console.WriteLine("estamos encontrando una resrva con el ejemplar que esta en prestamo");
                            resultado.Mensaje = "no puedes renovar el prestamo de este ejemplar ya que ha sido reservado por otro estudiante";
                            resultado.Icono = "info";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        Console.WriteLine("no esta encontrando reserva con el ejemplar del prestamo asi que se puede renovar");
                        resultado = await _prestamosServices.Editar(PrestamoExistente, User, Fecha_finDate);

                    }
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    return RedirectToAction(nameof(Index));

                }
                Console.WriteLine("no esta parseando el id del prestamo");
                return BadRequest(); // Retornar un código de error 400
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                Console.WriteLine($"Ocurrió un error en la función Edit (POST): {ex.Message}");
                return BadRequest(); // Retornar un código de error 400
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Id_peticion,Fecha_inicio,Fecha_fin,Estado")] Prestamo prestamo)
        {
            try
            {
                if (id != prestamo.Id)
                {
                    return NotFound();
                }

                // Inicializar el objeto resultado para almacenar el mensaje que se mostrará en la alerta
                ResponseModel resultado = new ResponseModel();

                // Buscar el prestamo actual al que pertenece el prestamo a editar
                var PrestamoExistente = await _prestamosServices.Buscar(prestamo.Id);

                if (PrestamoExistente != null)
                {
                    if (PrestamoExistente.Estado == "FINALIZADO")
                    {
                        resultado.Mensaje = "No puedes renovar un préstamo que ya ha sido finalizado";
                        resultado.Icono = "error";
                    }
                    else
                    {
                        Console.WriteLine("El prestamo está en curso");
                        
                        if (PrestamoExistente.Fecha_fin >= prestamo.Fecha_fin)
                        {
                            resultado.Mensaje = "No puedes poner la fecha final del préstamo antes de la actual registrada.";
                            resultado.Icono = "error";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        else if ((prestamo.Fecha_fin - PrestamoExistente.Fecha_fin).TotalDays > 15)
                        {
                            resultado.Mensaje = "La fecha final del préstamo no puede exceder  15 días de la fecha final del préstamo existente.";
                            resultado.Icono = "error";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        if (await _prestamosServices.ValidarReservaExistente(PrestamoExistente.Peticion.Ejemplar.Id))
                        {
                            Console.WriteLine("estamos encontrando una resrva con el ejemplar que esta en prestamo");
                            resultado.Mensaje = "no puedes renovar el prestamo de este ejemplar ya que ha sido reservado por otro estudiante";
                            resultado.Icono = "info";
                            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            Console.WriteLine("no esta encontrando reserva con el ejemplar del prestamo asi que se puede renovar");
                            resultado = await _prestamosServices.Editar(PrestamoExistente, User, prestamo.Fecha_fin);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No encontró el préstamo sabiendo que es el mismo id");
                }

                TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
    }
}