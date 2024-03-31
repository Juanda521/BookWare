using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;

namespace tallerbiblioteca.Services
{
    public class ConfiguracionServices
    {
        private readonly BibliotecaDbContext _context;
        private RolServices _rolServices;
        private PermisosServices _permisosServices;

        public ConfiguracionServices(BibliotecaDbContext context,RolServices rolServices)
        {
            _context = context;
            _rolServices = rolServices;
            _permisosServices = new PermisosServices(_context);
            
        }

        public List<Configuracion> ConfiguracionRol(int id)
        {
            return _context.Configuracion.Include(c => c.Permiso).Where(c => c.Id_rol == id).ToList();
        }

        public async Task<List<Rol>> ObtenerRoles()
        {
            return await _rolServices.ObtenerRoles();
        }

        public List<Permiso> ObtenerPermisos(){
             return  _permisosServices.ObtenerPermisos();
        }

        public void RegistrarExcel(Matriculados matriculado){
            _context.Matriculados.Add(matriculado);
            Console.WriteLine("lo ha agregado al contex");
        }

         public void guardarUsuariosFromExcel(){
            _context.SaveChanges();
            Console.WriteLine("los ha agregado a la base de datos");
        }

        public async Task<bool> ValidarEstudianteMatriculado(long numero_documento)
        {
            var matriculado = await _context.Matriculados.FirstOrDefaultAsync(m => m.Documento == numero_documento);
            if (matriculado != null)
            {
                Console.WriteLine($"el estudiante con numero de documento: {matriculado.Documento} ya existe en la base de datos del aplicativo");
            }
            else
            {
                Console.WriteLine($"el estudiante con numero de documento: {numero_documento} no existe en la base de datos del aplicativo asi que devolvemos false para registrarlo en el controlador");
            }
            return matriculado != null;
        }


        public List<Permiso> PermisosNoAsociados(int Id_rol)
        {
            return _context.Permisos.Where(p => !_context.Configuracion.Any(c => c.Id_permiso == p.Id && c.Id_rol == Id_rol)).ToList();
        }

        public async Task<List<Usuario>> UsuariosInactivos()
        {
           return await _context.Usuarios.Where(u => u.Estado == "INHABILITADO" || u.Estado == "SUSPENDIDO").ToListAsync();
        }

        public async Task<bool> Create(Configuracion configuracion)
        {
            Console.WriteLine("rol asignar:", configuracion.Id_rol);
            Console.WriteLine("permiso asignar:", configuracion.Id_permiso);
            _context.Configuracion.Add(configuracion);
            return (await _context.SaveChangesAsync() > 0) ? true : false;
        }

        public async Task<bool> Edit(int Id)
        {
            var config = await _context.Configuracion.FindAsync(Id);
            if (config != null)
            {
                Console.WriteLine(config.Estado);
                if (config.Estado.Equals("ACTIVO"))
                {
                    config.Estado = "INACTIVO";
                }
                else
                {
                    config.Estado = "ACTIVO";
                }
                _context.Update(config);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> Delete(int Id)
        {
            Console.WriteLine("este es el id a eliminar:", Id);
            var config = await _context.Configuracion.FindAsync(Id);
            if (config != null)
            {
                _context.Configuracion.Remove(config);
                
            }
            return (await _context.SaveChangesAsync() > 0) ? true : false;
        }

        public int ObtenerRolUserOnline(ClaimsPrincipal User){
            var Id_rol_string = User.FindFirst(ClaimTypes.Role).Value;  
            return  Int32.Parse(Id_rol_string);
        }
        
        public int ValidacionConfiguracionActiva(string nombre, int Id_rol)
        {

         
            int Status;
            var permiso = _context.Permisos.FirstOrDefault(p => p.Nombre == nombre);

         
            if (permiso != null)
            {
               
            
                var rol = _context.Configuracion.FirstOrDefault(c => c.Id_rol == Id_rol && c.Id_permiso == permiso.Id);

                if (rol != null)
                {
                   
                    var config = _context.Configuracion.FirstOrDefault(c => c.Id_permiso == permiso.Id);
                    if (config != null)
                    {
                        if (config.Estado == "ACTIVO")
                        {
                            //el rol del usuario en linea tiene acceso a relizar la accion y la configuracion se encuentra activa
                            Status = 200;
                            return Status;
                        }
                        //el usuario en linea tiene permitido realizar la accion pero el estado no se encuentra activo por el momento
                        Status = 402;
                        return Status;
                    }
                }
                //el permiso no esta asociado al rol del usuario en linea
                
                 
                Status = 401;
                return Status;
            }
            // no se encontro permiso con este nombre, es decir, no existe el permiso
            Status = 400; //dont found
            return Status;

        }

        public ResponseModel  MensajeRespuestaValidacionPermiso(int status){
            Console.WriteLine(status);
            var resultado = new ResponseModel();
            switch (status)
            {
                case 200:
                    resultado.Mensaje  =  "La accion se ha realizado con exito";
                    resultado.Icono  = "success";
                    // TempData["Mensaje"] = "La accion se ha realizado con exito";
                break;
                case 401:
                    resultado.Mensaje  =  "No tienes permiso para realizar esta accion";
                    resultado.Icono  = "error";
                 
                break;
                case 402:
                    resultado.Mensaje  = "El permiso para realizar esta accion no se encuentra activo";
                    resultado.Icono  = "info";
                break;
            }
            return  resultado;
        }
        public async Task<ConfiguracionViewModel> CrearViewModel()
        {
            //creamos un objeto de ConfiguracionViewModel para poder enviar varios modelos a la vista
            var detalles = new ConfiguracionViewModel();

            Permiso Permiso = new();

            //enviamos los roles y los permisos al objeto ViewModel para poder renderizar en la vista
            detalles.Roles = await ObtenerRoles();
            detalles.Permisos = await _context.Permisos.ToListAsync();

            detalles.Permiso = Permiso;
            return detalles;
        }

        public ConfiguracionViewModel MostrarConfiguracion(int Id_rol,ConfiguracionViewModel configuracionViewModel)
        {
            var configuracionesRol = ConfiguracionRol(Id_rol);
            if (configuracionesRol.Count > 0)
            {
                if (Id_rol == 01)
                {
                    if (configuracionViewModel.ConfiguracionesAdmin == null)
                    {
                        configuracionViewModel.ConfiguracionesAdmin = new List<Configuracion>(); // Inicializa la lista si es nula
                    }
                    configuracionViewModel.ConfiguracionesAdmin.AddRange(configuracionesRol);
                  

                }
                else if (Id_rol == 02)
                {
                    if (configuracionViewModel.ConfiguracionesUsuario == null)
                    {
                        configuracionViewModel.ConfiguracionesUsuario = new List<Configuracion>(); // Inicializamos la lista si es nula
                    }
                    configuracionViewModel.ConfiguracionesUsuario.AddRange(configuracionesRol);
                   
                }
                else if (Id_rol == 03)
                {
                    if (configuracionViewModel.ConfiguracionesAlfabetizador == null)
                    {
                        configuracionViewModel.ConfiguracionesAlfabetizador = new List<Configuracion>(); // Inicializamos la lista si es nula
                    }
                    configuracionViewModel.ConfiguracionesAlfabetizador.AddRange(configuracionesRol);
                  
                }

            }
            return configuracionViewModel;
        }

   

     
    }
}
