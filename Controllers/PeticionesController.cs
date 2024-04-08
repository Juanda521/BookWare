
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Security.Claims;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
namespace tallerbiblioteca.Controllers
{
    [Authorize]
    public class PeticionesController : Controller
    {
        private PeticionesServices _peticionesServices;
        private UsuariosServices _usuariosServices;
        private EjemplarServices _ejemplaresServices;
        private ReservasServices _reservasServices;
        public PeticionesController(UsuariosServices usuariosServices, PeticionesServices peticionesServices, EjemplarServices ejemplarServices,ReservasServices reservasServices)
        {
            _peticionesServices = peticionesServices;
            _usuariosServices = usuariosServices;
            _ejemplaresServices = ejemplarServices;
            _reservasServices = reservasServices;

        }

        // GET: Peticiones
        public async Task<IActionResult> Rechazadas(int itemsPagina = 5, int pagina = 1,DateTime? fechaini = null,DateTime? fechafin= null,string? busqueda=null)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                PeticionesViewModel peticionesViewModel = new()
                {
                    Peticiones = await _peticionesServices.Rechazadas()

                };
                if (busqueda != null || fechaini != null || fechaini != null)
                {
                    peticionesViewModel.Peticiones = await _peticionesServices.Buscarechazadas(fechaini, fechafin, busqueda);
                }
                peticionesViewModel.Peticiones = peticionesViewModel.Peticiones.OrderBy(p => p.Estado == "EN ESPERA" ? 0 : 1).ToList();
                int totalPeticiones = peticionesViewModel.Peticiones.Count;
                var total = (totalPeticiones / 6) + 1;

                var peticionesPaginadas = peticionesViewModel.Peticiones.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

                Paginacion<Peticiones> paginacion = new(peticionesPaginadas, total, pagina, itemsPagina)
                {
                    PeticionesViewModel = peticionesViewModel
                };

                return View("Rechazadas", paginacion);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
        public async Task<IActionResult> BuscarFiltro(int itemsPagina = 5, int pagina = 1, string busqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            PeticionesViewModel peticionesViewModel = new()
            {
                Peticiones = await _peticionesServices.BuscarObtener(busqueda, fechaInicio, fechaFin)
            };
            int totalPeticiones = peticionesViewModel.Peticiones.Count;
            var total = (totalPeticiones / 6) + 1;

            var peticionesPaginadas = peticionesViewModel.Peticiones.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

            Paginacion<Peticiones> paginacion = new(peticionesPaginadas, total, pagina, itemsPagina)
            {
                PeticionesViewModel = peticionesViewModel
            };


            return View("Index", paginacion);
        }
        public async Task<IActionResult> Index(int itemsPagina = 5, int pagina = 1, int? id = null, string busqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
               
                PeticionesViewModel peticionesViewModel = new()
                {
                    Peticiones = await _peticionesServices.Obtenerpeticiones()

                };
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (User.IsInRole("2") && userIdClaim != null)
                {
                    int userId = Convert.ToInt32(userIdClaim.Value);
                    peticionesViewModel.Peticiones = await _peticionesServices.BuscarP(userId);
                }
                if (busqueda != null || fechaInicio != null || fechaFin != null)
                {
                    peticionesViewModel.Peticiones = await _peticionesServices.BuscarFiltro(busqueda, fechaInicio, fechaFin);
                }
                peticionesViewModel.Peticiones = peticionesViewModel.Peticiones.OrderBy(p => p.Estado == "EN ESPERA" ? 0 : 1).ThenByDescending(p => p.FechaPeticion).ToList();
                int totalPeticiones = peticionesViewModel.Peticiones.Count;
                var total = (totalPeticiones / 6) + 1;

                var peticionesPaginadas = peticionesViewModel.Peticiones.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();
                var ejemplares = await _ejemplaresServices.ObtenerEjemplaresDisponibles(); // Asegúrate de que tu servicio devuelva ejemplares con información de libro
                ViewData["Id_ejemplar"] = new SelectList(ejemplares, "Id", "Libro.Nombre"); // Usa el nombre de la propiedad que contiene el nombre del libro
                ViewData["Id_usuario"] = new SelectList(await _usuariosServices.ValidarUsuario(), "Id", "Name");
                var model = new PeticionesModel(new Paginacion<Peticiones>(peticionesPaginadas, total, pagina, itemsPagina), new Peticiones());
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
            }
            
        //este es el metodo que nos devuelve las notificaciones que van aparecer en la campana del aplicativo
        [HttpGet]
        [Route("api/Notificaciones")]
        public async Task<List<Peticiones>> Notificaciones()
        {
            return await _peticionesServices.ObtenerpeticionesEnEspera();
        }


        private void MensajeRespuestaValidacionPermiso(ResponseModel resultado, int status)
        {
            Console.WriteLine("con este status vamos a mostrara la alerta:" + status);
            switch (status)
            {
                case 200:
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 401:
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 402:
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
            }
        }


        private void MensajeRespuestaPeticion(int status)
        {
            Console.WriteLine(status);
            var resultado = new ResponseModel();
            switch (status)
            {
                case 200:
                    string Mensaje = "";
                    if(_peticionesServices.ObtenerRolUserOnline(User)==1 ||_peticionesServices.ObtenerRolUserOnline(User)==3 ){
                        Mensaje = "La acción se ha realizado con exito, Puedes aceptar o rechazar esta petición en el apartado de peticiones";
                    }else{
                        Mensaje = "La accion se ha realizado con exito, Revise el correo en un lapso de 24 horas para la confirnmacion de su solicitud";
                    }
                    resultado.Mensaje = Mensaje;
                    resultado.Icono = "success";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 500:
                    resultado.Mensaje = "No puedes realizar esta acción ya que tienes una solicitud pendiente";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 402:
                    resultado.Mensaje = "El permiso para realizar esta accion no se encuentra activo";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 501:
                    resultado.Mensaje = "Estas Inhabilitado o Suspendido, no puedes realizar esta accion, comunicate con el administrador para que puedas acceder a este servicio";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                break;
                case 502:
                    resultado.Mensaje = "El libro solicitado no se encuentra Disponible, puedes reservarlo para que puedad disfrutar de el cuando esté disponible de nuevo en la biblioteca";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 503:
                    resultado.Mensaje = "No puedes realizar esta acción ya que tienes un prestamo en curso";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                 case 504:
                    resultado.Mensaje = "No puedes realizar esta acción ya que este usuario tiene un prestamo en curso";
                    resultado.Icono = "info";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                    break;
                case 505:
                    resultado.Mensaje = "No puedes realizar esta acción ya que la peticion ya ha sido aceptada";
                    resultado.Icono = "error";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                     break;
                case 506:
                    resultado.Mensaje = "No puedes realizar esta acción porque  la peticion ya ha sido Rechazada";
                    resultado.Icono = "error";
                    TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
                     break;
                default:
                    Console.WriteLine("i'm failing in the name of permission");
                    break;
            }

        }

        // GET: Peticiones/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }

                if (await _peticionesServices.Obtenerpeticiones() == null)
                {
                    return NotFound();
                }
                var peticion = await _peticionesServices.Buscar(id);
                if (peticion == null)
                {
                    return NotFound();
                }
                return View(peticion);
            }catch(Exception ) {
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var ejemplares = await _ejemplaresServices.ObtenerEjemplaresDisponibles(); 
            ViewData["Id_ejemplar"] = new SelectList(ejemplares, "Id", "Libro.Nombre"); 
            ViewData["Id_usuario"] = new SelectList(await _usuariosServices.ValidarUsuario(), "Id", "Name");
            return View();
        }
        // POST: Peticiones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Peticiones peticiones,int? idReserva = null)
        {
            Console.WriteLine("id del ejemplar a registrar. vamos a registrar una fokin petición ", peticiones.Id_ejemplar);
            try
            {
                if (_usuariosServices.ValidarUsuarioEnPrestamo(peticiones.Id_usuario) == true && idReserva == null)
                {
                    Console.WriteLine("Esta usuario tiene un prestamo en curso con id de " + peticiones.Id_usuario);
                    MensajeRespuestaPeticion(504);
                    return RedirectToAction("Index", "Peticiones");
                }
                else if (_usuariosServices.ValidarUsuarioEnPrestamo(peticiones.Id_usuario) == true && idReserva != null)
                {
                    MensajeRespuestaPeticion(504);
                    return RedirectToAction("Index", "Reservas");
                }

                int status;
                if (idReserva != null)
                {
                    Console.WriteLine("ENTRANDO DESDE VISTA RESERVA");
                    status = await _peticionesServices.RegistrarR(User, peticiones, idReserva);
                }
                else
                {
                    Console.WriteLine("Desde peticiones");
                    status = await _peticionesServices.Registrar(_peticionesServices.ObtenerRolUserOnline(User), peticiones);
                }


                if (status == 401 || status == 402)
                {
                    var resultado = new ResponseModel();
                    MensajeRespuestaValidacionPermiso(_peticionesServices.MensajeRespuestaValidacionPermiso(status), status);
                }
                else
                {
                    MensajeRespuestaPeticion(status);
                }
                return RedirectToAction("Catalog", "Libros");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        

        [HttpPost]
        public async Task<IActionResult> Registrar()
        {
            try
            {
                Console.WriteLine("hablalo desde registrar");
                string idEjemplar = Request.Form["Id_ejemplar"];
                string motivo = Request.Form["Motivo"];
                string idUsuario = Request.Form["Id_usuario"];
                Console.WriteLine("aca deberia copier el id: {0} ", idEjemplar);


                if (int.TryParse(idEjemplar, out int idEjemplarInt))
                {
                    Console.WriteLine("id del ejemplar a registrar: {0}", idEjemplarInt);
                }
                else
                {
                    MensajeRespuestaPeticion(502);
                    Console.WriteLine("no esta parseando el ejemplar");
                    return RedirectToAction("Catalog", "Libros");
                }
                if (int.TryParse(idUsuario, out int idUsuarioInt))
                {

                    Console.WriteLine("id del usuario a registrar: {0}", idUsuarioInt);
                }
                if (_usuariosServices.ValidarUsuarioEnPrestamo(idUsuarioInt))
                {

                    Console.WriteLine("Esta usuario tiene un prestamo en curso");
                    //codigo 503 declarado para usuarios con prestamos en curso 
                    MensajeRespuestaPeticion(503);
                    return RedirectToAction("Catalog", "Libros");
                }

                Peticiones peticiones = new Peticiones();
                peticiones.Id_ejemplar = idEjemplarInt;
                peticiones.Id_usuario = idUsuarioInt;
                peticiones.Motivo = motivo;

                int Id_rol = _peticionesServices.ObtenerRolUserOnline(User);
                Console.WriteLine(Id_rol);

                int status = await _peticionesServices.Registrar(Id_rol, peticiones);
                Console.WriteLine(status);

                if (status == 401 || status == 402)
                {
                    var resultado = new ResponseModel();
                    MensajeRespuestaValidacionPermiso(_peticionesServices.MensajeRespuestaValidacionPermiso(status), status);
                }
                else
                {
                    MensajeRespuestaPeticion(status);
                }
                return RedirectToAction("Catalog", "Libros");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [Route("Movil/RegistrarPeticion")]
        [AllowAnonymous]
        public async Task<IActionResult> RegistrarMovil([FromBody]PeticionViewModel viewModel){

            Console.WriteLine("hola mundo");
            Console.WriteLine(viewModel.Id_rol);
            if(_usuariosServices.ValidarUsuarioEnPrestamo(viewModel.Peticion.Id_usuario)){

                Console.WriteLine("Este usuario tiene un prestamo en curso");
                return StatusCode(503,"este usuario tiene un prestamo en curso");
            }else if (await _peticionesServices.ValidacionPeticionPendiente(viewModel.Peticion)){
                return StatusCode(500,"ya existe una peticion en espera de este usuario");
            }else{
                if(await _peticionesServices.ValidarEjemplarEnPeticion(viewModel.Peticion.Id_ejemplar)){

                    if (await _usuariosServices.ValidarUsuarioEnReserva(viewModel.Peticion.Id_usuario)){
                        return StatusCode(502,"ya existe una reserva asociada a este usuario");
                    };
                    Reserva reserva = new(){
                        IdEjemplar  = viewModel.Peticion.Id_ejemplar,
                        IdUsuario = viewModel.Peticion.Id_usuario
                    };
                    Console.WriteLine($"este es el id del usuario de la reserva: {reserva.IdUsuario}");
                    int status = await _reservasServices.RegistrarMovil(reserva, viewModel.Id_rol);
                    Console.WriteLine(status);

                    if (status == 401 || status == 402)
                    {
                        // var resultado = new ResponseModel();
                        // MensajeRespuestaValidacionPermiso(_peticionesServices.MensajeRespuestaValidacionPermiso(status), status);
                        return Unauthorized("el usuario en linea no tiene permiso de realizar la accion");
                    }
                    return Ok("se ha registrado la reserva exitosamente");

                }else{
                    if (await _usuariosServices.ValidarUsuarioEnReserva(viewModel.Peticion.Id_usuario)){
                        return StatusCode(502,"ya existe una reserva asociada a este usuario");
                    };

                    viewModel.Peticion.Motivo = "Prestamo libro";
                    int status = await _peticionesServices.Registrar(viewModel.Id_rol, viewModel.Peticion);
                    Console.WriteLine(status);

                    if (status == 401 || status == 402)
                    {
                        // var resultado = new ResponseModel();
                        // MensajeRespuestaValidacionPermiso(_peticionesServices.MensajeRespuestaValidacionPermiso(status), status);
                        return Unauthorized("el usuario en linea no tiene permiso de realizar la accion");
                    }
                    else if (status == 500)
                    {
                        return StatusCode(500,"ya existe una peticion en espera de este usuario");
                        // MensajeRespuestaPeticion(status);
                    }else{
                        return Ok("se ha registrado la peticion exitosamente");
                    }
                }
            }
           
            
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Console.WriteLine("vamos a cambiar el estado de la petición");

                var peticion = await _peticionesServices.Buscar(id);

                if (peticion.Estado == "ACEPTADA")
                {
                    Console.WriteLine($"La peticion ya ha sido aceptada");
                    MensajeRespuestaPeticion(505);
                    return RedirectToAction(nameof(Index));
                }

                if (peticion.Estado == "RECHAZADA")
                {
                    Console.WriteLine($"La peticion ya ha sido rechazada");
                    MensajeRespuestaPeticion(506);
                    return RedirectToAction(nameof(Rechazadas));
                }


                int status = await _peticionesServices.EliminarPeticion(User, id);
                MensajeRespuestaValidacionPermiso(_peticionesServices.MensajeRespuestaValidacionPermiso(status), status);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

    }
}

