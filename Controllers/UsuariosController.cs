using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using System.Net.Mail;
using System.Numerics;


namespace tallerbiblioteca.Controllers
{
    [Authorize]
    public class Usuarios : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly UsuariosServices _usuariosServices;
        private readonly ReservasServices _reservasServices;
        private readonly PrestamosServices _prestamosServices;
        private ConfiguracionServices _configuracionServices;
        private int Status;
        public Usuarios(BibliotecaDbContext context, UsuariosServices usuariosServices, ReservasServices reservasServices, PrestamosServices prestamosServices, ConfiguracionServices configuracionServices)
        {
            _context = context;
            _usuariosServices = usuariosServices;
            _reservasServices = reservasServices;
            _prestamosServices = prestamosServices;
            _configuracionServices = configuracionServices;
        }

        [AllowAnonymous]
        public IActionResult login()
        {
            return View();
        }
        [AllowAnonymous]
        // GET: Usuarios/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Usuarios
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
                var usuarios = await _usuariosServices.ObtenerUsuarios();

                if (busqueda != null)
                {
                    usuarios = await _usuariosServices.Buscar(busqueda);
                }

                usuarios = usuarios.OrderBy(p => p.Estado == "ACTIVO" ? 0 : 1).ToList();
                var totalUsuarios = usuarios.Count();
                var total = (totalUsuarios / 6) + 1;

                var usuariosPaginados = usuarios.Skip((pagina - 1) * itemsPagina).Take(itemsPagina).ToList();

                ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                var model = new usuarioModel(new Paginacion<Usuario>(usuariosPaginados, total, pagina, itemsPagina), new Usuario());
                return View(model);
            }
            catch (Exception)
            {

                return RedirectToAction("Error");
            }
        }
        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {

                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                int status = _usuariosServices.VistaEdit(User);
                if (status == 200)
                {
                    var usuario = await _context.Usuarios.FindAsync(id);
                    return View(usuario);
                }
                else
                {
                    MensajeRespuestaValidacionPermiso(status);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
        //cambiar if a switch
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
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Inhabilitar(int id)
        {
            try
            {
                MensajeRespuestaValidacionPermiso(_usuariosServices.Inhabilitar(id, User));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Suspender(int id)
        {
            try
            {
                MensajeRespuestaValidacionPermiso(_usuariosServices.Suspender(id, User));

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }

        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([Bind("Numero_documento,Contraseña")] Usuario usuario)
        {
            try
            {
               
                if (_context.Usuarios.FirstOrDefault(u => u.Numero_documento == usuario.Numero_documento) != null)
                {
                    Console.WriteLine(usuario.Contraseña);
                    Console.WriteLine(usuario.Numero_documento);
                    var UserEncontrado = _usuariosServices.BuscarUsuario(usuario.Numero_documento, usuario.Contraseña);
                    if (UserEncontrado != null)
                    {
                        Console.WriteLine("encontro al usuario");
                        if (UserEncontrado.Estado == "INHABILITADO")
                        {
                            
                            return StatusCode(403, new { message = "No puedes ingresar al aplicativo, te encuentras Inhabilitado" });
                        }
                        else if (UserEncontrado.Estado == "SUSPENDIDO")
                        {
                            
                            return StatusCode(403, new { message = "No puedes ingresar al aplicativo, te encuentras Suspendido" });

                        }
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,UserEncontrado.Name),
                           new Claim(ClaimTypes.Email,UserEncontrado.Correo),
                              new Claim(ClaimTypes.Role,UserEncontrado.Id_rol.ToString()),
                                new Claim(ClaimTypes.NameIdentifier,UserEncontrado.Id.ToString()),
                                   new(ClaimTypes.Role, UserEncontrado.Rol.Nombre),
                    };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            
                        };

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        // ViewData["Iniciar"] = "true";
                        return Ok(new { success = true }); 
                    }
                    else
                    {
                        return BadRequest(new { error = "Credenciales incorrectas." }); 
                    }
                }
                else
                {
                    
                    return NotFound(new { error = "Usuario no encontrado." });
                }
                return NotFound(new { error = "Usuario no encontrado." });
            }catch(Exception) {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [Route("Movil/Login/")]
        [AllowAnonymous]
        public async Task<IActionResult>LoginMovil([FromBody] Login loginData)
        {
          
            Console.WriteLine($"Numero de documento: {loginData.Numero_documento}");
            Console.WriteLine($"Contraseña: {loginData.Contraseña}");

            var UserEncontrado = _usuariosServices.BuscarUsuario(loginData.Numero_documento,loginData.Contraseña);

            if(UserEncontrado!=null){
                
                return Ok("las credenciales son validas y se procede a realizar el login");
            }else{
                return StatusCode(503,"las credenciales no coiniciden");
            }


        
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Name,Apellido,Correo,Numero_documento,Contraseña")] Usuario usuario)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                MensajeRespuestaValidacionPermiso(await _usuariosServices.Edit(usuario));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                var host = addr.Host;
                var mxRecords = System.Net.Dns.GetHostAddresses(host)
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return mxRecords.Any();
            }
            catch
            {
                return false;
            }
        }
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Id_rol,Numero_documento,Name,Apellido,Correo,Contraseña,Estado")] Usuario usuario)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.Role);
                var nopasar = false;
                Console.WriteLine("ESTAMOS VALIDANDO TODO");
                bool esMatriculado = false;
                if (userId != "1" && userId!="3")
                {
                    Console.WriteLine("VALIDANDO DOCUMENTO EN MATRICULADOS");
                    BigInteger bigint = BigInteger.Parse(usuario.Numero_documento.ToString());
                    esMatriculado = await _usuariosServices.ValidacionMatriculado(bigint);
                    if (esMatriculado == false)
                    {

                        ViewData["Nomatriculado"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);

                    }
                    var nombreExistente = await _usuariosServices.ValidarNombreExistente(usuario.Numero_documento, usuario.Name);
                    if (nombreExistente == true)
                    {
                        Console.WriteLine("ABRIR SWEET ALERT");
                        ViewData["NombreExistente"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);
                    }
                    var validarApellido = await _usuariosServices.ValidarApellidoExistente(usuario.Numero_documento, usuario.Apellido);
                    if (validarApellido == true)
                    {
                        Console.WriteLine("ABIRR SWEET ALERT APELLIDO");
                        ViewData["ApellidoExistente"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);
                    }
                    BigInteger bigintV = BigInteger.Parse(usuario.Numero_documento.ToString());
                    bool Encontrar = _usuariosServices.validarDocumento(bigintV);
                    if (Encontrar)
                    {
                        nopasar = true;
                        Console.WriteLine("Encontrado, no dejar entrar");
                        ViewData["Encontrados"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);
                    }
                    if (_usuariosServices.validarCorreo(usuario.Correo))
                    {
                        nopasar = true;
                        Console.WriteLine("Encontrado, no dejar entrar");
                        ViewData["Encontrado"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);
                    }
                    if (!IsValidEmail(usuario.Correo))
                    {
                        nopasar = true;
                        Console.WriteLine("Correo electrónico no válido, no dejar entrar");
                        ViewData["CorreoInvalido"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);
                    }
                }
                if (esMatriculado == true || userId == "1" || userId == "3")
                {
                    BigInteger bigintV = BigInteger.Parse(usuario.Numero_documento.ToString());
                    bool Encontrar = _usuariosServices.validarDocumento(bigintV);
                    if (Encontrar)
                    {
                        Console.WriteLine("Encontrado Documento existente, no dejar entrar");
                        ViewData["Encontrados"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);

                    }

                    if (_usuariosServices.validarCorreo(usuario.Correo))
                    {
                        nopasar = true;
                        Console.WriteLine("NO DEBERÍA REDIRIGIR correo encontrado ");
                        ViewData["Encontrado"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);

                    }
                    if (!IsValidEmail(usuario.Correo))
                    {
                        Console.WriteLine("NO DEBERÍA REDIRIGIR CORREO NO VÁLIDO ");

                        nopasar = true;
                        Console.WriteLine("Correo electrónico no válido, no dejar entrar");
                        ViewData["CorreoInvalido"] = "True";
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");
                        return View("Create", usuario);


                    }
                    if (nopasar == false)
                    {
                        Console.WriteLine("Se encontró el documento dejar entrar");

                        if (_context.Rol.Any(r => r.Id == usuario.Id_rol))
                        {

                            await this._usuariosServices.Create(usuario);
                            MensajeRespuestaValidacionPermiso(200);
                            
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("Id_rol", "El rol seleccionado no es válido.");
                        }
                        ViewData["Id_rol"] = new SelectList(_context.Rol, "Id", "Nombre");

                    }
                    return View("Create");
                }
                else
                {
                    nopasar = true;
                    Console.WriteLine("No se encontró el documento en matriculado, no dejar entrar");
                    ViewData["Nomatriculado"] = "True";
                    return View("Create", usuario);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
        public IActionResult eliminarsan(int id)
        {
            try
            {
                var usuarios = _context.Usuarios.Find(id);

                if (usuarios != null)
                {
                    usuarios.Estado = "ACTIVO";
                    _context.SaveChanges();
                }
                return RedirectToAction("Index");
            }catch(Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> buscar(string filtro)
        {
            try
            {
                var buscar = await _context.Usuarios
                    .Where(u => u.Name.Contains(filtro) || u.Apellido.Contains(filtro) || u.Correo.Contains(filtro))
                    .ToListAsync();

                return View("Index", buscar);
            } catch (Exception) 
            {
                return RedirectToAction("Error");
            }

        }
      
        public async Task<IActionResult> Informacion(int id)
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                var prestamosEnCurso = _context.Prestamos
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Usuario)
                    .Include(p => p.Peticion)
                        .ThenInclude(p => p.Ejemplar)
                            .ThenInclude(p => p.Libro)
                    .Where(k => k.Peticion.Usuario.Id == id && k.Estado == "En curso")
                    .ToList();

                var buscusu = _context.Usuarios.FirstOrDefault(t => t.Id == id);

                if (prestamosEnCurso.Any())
                {
                    foreach (var presta in prestamosEnCurso)
                    {
                        var usuario = new usuarioo
                        {
                            Mensaje = "Tienes un préstamo en curso ",
                            NombreLibro = $"Nombre del libro en préstamo: {presta.Peticion.Ejemplar.Libro.Nombre}",
                            fechaa = $"Tiene una fecha de entrega para el día: {presta.Fecha_fin}",
                        };
                        usuario.Name = buscusu.Name;
                        usuario.Apellido = buscusu.Apellido;
                        usuario.Correo = buscusu.Correo;
                        usuario.Numero_documento = buscusu.Numero_documento;
                        return View("Informacion", usuario);
                    }
                }
                var peticionesEnEspera = _context.Peticiones
                    .Include(p => p.Usuario)
                    .Include(p => p.Ejemplar)
                        .ThenInclude(e => e.Libro)
                    .Where(p => p.Estado == "EN ESPERA" && p.Id_usuario == id)
                    .ToList();

                if (peticionesEnEspera.Any())
                {
                    foreach (var peticion in peticionesEnEspera)
                    {
                        var usuarioE = new usuarioo
                        {
                            Mensaje = "Tienes una petición pendiente ",
                            NombreLibro = $"Nombre del libro en petición: {peticion.Ejemplar.Libro.Nombre}",
                            fechaa = $"Está en espera desde el día :  {peticion.FechaPeticion}",
                        };
                        usuarioE.Name = peticion.Usuario.Name;
                        usuarioE.Apellido = peticion.Usuario.Apellido;
                        usuarioE.Correo = peticion.Usuario.Correo;
                        usuarioE.Numero_documento = peticion.Usuario.Numero_documento;
                        return View("Informacion", usuarioE);
                    }
                }

                var reserva = _context.Reserva
                    .Include(r => r.Ejemplar)
                        .ThenInclude(l => l.Libro)
                    .Include(r => r.Usuario)
                    .FirstOrDefault(u => u.IdUsuario == id && u.Estado == "ACTIVO");

                if (reserva != null)
                {
                    var usuarioaR = new usuarioo
                    {
                        Mensaje = "Tienes una reserva pendiente",
                        NombreLibro = $"Nombre del libro en reserva: {reserva.Ejemplar.Libro.Nombre}",
                        fechaa = $"Fecha de la reserva: {reserva.FechaReserva}"
                    };
                    usuarioaR.Name = buscusu.Name;
                    usuarioaR.Apellido = buscusu.Apellido;
                    usuarioaR.Correo = buscusu.Correo;
                    usuarioaR.Numero_documento = buscusu.Numero_documento;
                    return View("Informacion", usuarioaR);
                }
                else
                {

                    var usuarioa = new usuarioo
                    {
                        Mensaje = "No tienes nada pendiente"
                    };
                    usuarioa.Name = buscusu.Name;
                    usuarioa.Apellido = buscusu.Apellido;
                    usuarioa.Correo = buscusu.Correo;
                    usuarioa.Numero_documento = buscusu.Numero_documento;
                    return View("Informacion", usuarioa);
                }
            }catch(Exception) {
                return RedirectToAction("Error");
            }
            // Si no se cumple ninguna condición anterior, regresar a la vista por defecto
            
        }

        [AllowAnonymous]
        public IActionResult Recuperar()
        {
            try
            {
                return View("RecuperarContraseña");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RecuperarContraseña()
        {
            try
            {
                int codigo = 0;

                ResponseModel resultado = new();
                string numero_documento = Request.Form["Numero_documento"];
                Console.WriteLine(numero_documento);
                if (Int32.TryParse(numero_documento, out Int32 numero_documento_int))
                {
                    Console.WriteLine("vamos a parsear el numero de documento");
                    var (codigoServicio, mensajeError, UsuarioServicios) = _usuariosServices.RecuperarContraseña(numero_documento_int);
                    codigo = codigoServicio;
                    Usuario usuario = UsuarioServicios;
                    if (!string.IsNullOrEmpty(mensajeError))
                    {
                        ViewData["ErrorMessage"] = mensajeError;
                        return View();
                    }
                    else
                    {
                        HttpContext.Session.SetString("CodigoFinal", codigo.ToString());
                        var usuarioJson = JsonConvert.SerializeObject(usuario);
                        HttpContext.Session.SetString("Usuario", usuarioJson);
                        ViewData["Encontrados"] = "True";
                    }
                }
                else
                {
                    Console.WriteLine("no esta parseando el numero de documento");
                    ViewData["ErrorMessage"] = "El número de documento no es válido.";
                    return View();
                }

                Console.WriteLine($"Este es el codigo que genera{codigo}");

                return View("ConfirmarCodigo");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmarCodigo()
        {
            try
            {
                ResponseModel resultado = new();
                string codigo = Request.Form["codigo"];
                Console.WriteLine(codigo);
                if (Int32.TryParse(codigo, out Int32 codigo_int))
                {
                    int codigoGenerado = Convert.ToInt32(HttpContext.Session.GetString("CodigoFinal"));
                    Console.WriteLine($"Este es el codigo que ingrese en el formulario{codigo_int}");
                    Console.WriteLine($"Este es el codigo que genera los servicios{codigoGenerado}");
                    if (codigo_int == codigoGenerado)
                    {
                        Console.WriteLine("LISTO, NOS VAMOS A RESTABLECER");
                        return View("RestablecerContraseña");
                    }

                }
                ViewData["ErrorMessage"] = "No coinciden los Numeros, Revisa de nuevo por favor";
                return View();
            }
            catch
            {
                return RedirectToAction("Error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> restablecer(string contraseña)
        {
            try
            {
                Console.WriteLine(contraseña);

                if (!_usuariosServices.ValidarPassword(contraseña))
                {
                    ViewData["ErrorMessage"] = "La contraseña debe contener almenos una mayúscula,una minúscula y 8 o más caracteres";
                    return View("restablecerContraseña");
                }

                var usuarioJson = HttpContext.Session.GetString("Usuario");

                var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);
                var encryp = _usuariosServices.Encryptar(contraseña);
                var usuariomod = _context.Usuarios.Find(usuario.Id);
                usuariomod.Contraseña = encryp;
                _context.SaveChanges();
                if (usuariomod != null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado.";
                    return View("Login");

                }
                return View("Login");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> UsuariosInactivos()
        {
            try
            {
                var rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;
                if (rolUsuario == "2")
                {
                    Console.WriteLine("El rol del usuario es " + rolUsuario);
                    return View("Error");
                }
                var usuarios = await _usuariosServices.UsuariosInactivos();
                ViewBag.UsuariosInactivos = usuarios.Count;
                return View(usuarios);
            }
            catch (Exception )
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Perfil()
        {
            Console.WriteLine("Esat entrando a el perfil");
      
            var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuarios = await _usuariosServices.ObtenerUsuarios();
            var user = usuarios.FirstOrDefault(u => u.Id.ToString() == idUsuarioActual);
            Console.WriteLine("Contraseña antes " + user.Contraseña);

            user.Contraseña = _usuariosServices.Encryptar(user.Contraseña);
            Console.WriteLine("Contraseña después " + user.Contraseña);
            if (user != null)
            {
 
                Console.WriteLine("Esta entrnado a la vista");
                return View(user);
                // return RedirectToAction("Perfil", "Usuario");
            }
            else
            {
                // Manejo de errores, por ejemplo, redireccionar a una página de error o al inicio de sesión
                return RedirectToAction("Index", "Usuario");
            }
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CambiarRol(int id, int Rol)
        {
            try
            {
                int status = _configuracionServices.ValidacionConfiguracionActiva("Editar_Usuario", _configuracionServices.ObtenerRolUserOnline(User));
                if (status == 200)
                {
                    MensajeRespuestaValidacionPermiso(status);
                    var buscusu = _context.Usuarios.FirstOrDefault(t => t.Id == id);
                    Console.WriteLine("El rol del usuario es " + buscusu.Id_rol);
                    Console.WriteLine("El rol que llega es " + Rol);
                    buscusu.Id_rol = Rol;
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                return View("Index");
            }catch(Exception ) 
            {
                return RedirectToAction("Error");
            }
     
        }
        public async Task<IActionResult> EditarContra()
        {
            return View();

        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ValidarContras(string Contraseña)
        {
            var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("ID USUARIO ACTUAL "+idUsuarioActual);
            Console.WriteLine("Así llega la clave "+Contraseña);
            var encryp = _usuariosServices.Encryptar(Contraseña);
            Console.WriteLine("Y así es después de encriptarla : " + encryp);

            var user = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == idUsuarioActual);
            if (user != null)
            {
                Console.WriteLine("SE ENCONTRÓ EL USUARIO");
                Console.WriteLine("CONTRASEÑA DEL USUARIO " + user.Contraseña);
                Console.WriteLine("CONTRASEÑA ENCRIPTADA QUE LLEGÓ " + encryp);
                if (user.Contraseña != encryp)
                {
                    Console.WriteLine("NO COINCIDE UNA VERGA, ABRIR SWEET ALERT ");
                    ViewData["ContraseñaIn"] = "True";
                    return View("EditarContra");
                }
                else
                {
                    ViewData["ContraseñaCo"] = "True";
                }
            }
            
            return View("EditarContra");
        }
        public async Task<IActionResult> EditarContraseña()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> EditarContraseña(string Contraseña)
        {
            Console.WriteLine("LLEGÓ "+Contraseña);
            var idUsuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var encryp = _usuariosServices.Encryptar(Contraseña);
            var user = _context.Usuarios.FirstOrDefault(u => u.Id.ToString() == idUsuarioActual);
            
            if (user != null)
            {
                if (user.Contraseña == encryp)
                {
                    ViewData["Igual"] = "True";
                    return View();
                }
                else if(user.Contraseña != encryp)
                {
                    Console.WriteLine("ACTUALIZADO");
                    user.Correo = user.Correo;
                    user.Name = user.Name;
                    user.Numero_documento = user.Numero_documento;
                    user.Apellido = user.Apellido;
                    user.Id_rol = user.Id_rol;
                    user.Id = user.Id;
                    user.Contraseña = encryp;
                    ViewData["Editado"] = "True";
                    await _context.SaveChangesAsync();
                    return View("Perfil", user);
                   
                }
            }   
            return View("Perfil");
        }
        public async Task<ActionResult> EditPerfil(int id, [Bind("Id,Name,Apellido,Correo,Numero_documento,Contraseña")] Usuario usuario)
        {
            Console.WriteLine("Contraseña " + usuario.Contraseña);

            // Validar el formato del correo electrónico
            if (!IsValidEmail(usuario.Correo))
            {
                Console.WriteLine("Correo electrónico no válido");
                ViewData["CorreoInvalido"] = "True";
                return View("Perfil");
            }

            // Obtener el usuario actual
            var usuarioActual = await _context.Usuarios.FindAsync(id);

            // Comparar correos electrónicos sin importar mayúsculas/minúsculas
            if (string.Equals(usuario.Correo, usuarioActual.Correo, StringComparison.OrdinalIgnoreCase))
            {
                // El correo es igual al del usuario actual, proceder con la actualización
                Console.WriteLine("EL CORREO ES EL MISMO");
                MensajeRespuestaValidacionPermiso(await _usuariosServices.EditPerfil(usuario));
                return RedirectToAction("Catalog", "Libros");

            }
            else
            {
                bool correoExiste = _context.Usuarios.Any(u => u.Correo.ToLower() == usuario.Correo.ToLower());


                if (correoExiste)
                {
                    Console.WriteLine("El correo ya existe");
                    ViewData["CorreoExistente"] = "True";
                    return View("Perfil");
                }
                else
                {
                    MensajeRespuestaValidacionPermiso(await _usuariosServices.EditPerfil(usuario));
                    return RedirectToAction("Catalog", "Libros");

                }
            }
        }
    }
}


