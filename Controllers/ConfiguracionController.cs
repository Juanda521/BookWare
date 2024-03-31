using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace tallerbiblioteca.Controllers
{
    [Authorize(Roles = "1")]
    public class ConfiguracionController : Controller
    {
       
        private readonly ConfiguracionServices _configServices;
       
    
        public ConfiguracionController(ConfiguracionServices configuracionServices)
        {
            _configServices =  configuracionServices;    
        }

        //[HttpPost]
        //public async Task<ActionResult> SubirArchivoMartriculados(IFormFile archivoCSV)
        //{
        //    Console.WriteLine("llegamos a la funcion ");
        //    if (archivoCSV != null && archivoCSV.Length > 0)
        //    {
        //        Console.WriteLine("tenemos el archivo csv");
        //        int contador = 0;
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            archivoCSV.CopyTo(memoryStream);
        //            memoryStream.Seek(0, SeekOrigin.Begin); // Reiniciar el puntero del flujo de memoria

        //            using (var reader = new StreamReader(memoryStream))
        //            {
        //                ResponseModel res = new();
        //                while (!reader.EndOfStream)
        //                {
        //                    var line = reader.ReadLine();
        //                    var values = line.Split(';'); // Puedes ajustar el separador según el formato del CSV

        //                    if (values.Length == 3) // Asegúrate de tener al menos 3 valores por fila (Documento, Nombre, Apellido)
        //                    {


        //                        long documento;
        //                        if (long.TryParse(values[0], out documento))
        //                        {

        //                            if(await _configServices.ValidarEstudianteMatriculado(documento)){
        //                                Console.WriteLine("no vamos a registrar el estudiante porque ya existe en la base de datos ");
        //                            }else{

        //                                Console.WriteLine("vamos a crear el estudiante ya que no existe en la base de datos");
        //                                var matriculado = new Matriculados
        //                                {
        //                                    Documento = documento,
        //                                    Nombre = values[1],
        //                                    Apellido = values[2]
        //                                };
        //                                Console.WriteLine($"se supone que creamos un matriculado con el sguiente numero de documento: {matriculado.Documento} ");
        //                                _configServices.RegistrarExcel(matriculado);
        //                                contador ++;
        //                            }


        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine("No se pudo convertir el documento a long");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Formato incorrecto de fila en el archivo CSV");

        //                        res.Mensaje = "el archivo CSV no cumple con las condiciones estipuladas, recuerda debe tener solamente 3 columnas";
        //                        res.Icono = "error";
        //                        MensajeRespuestaValidacionPermiso(res);
        //                        return RedirectToAction(nameof(Index));
        //                    }
        //                }
        //                ResponseModel resultado = new();
        //                if(contador>0){

        //                    resultado.Mensaje = $"han sido cargados {contador} estudiantes a la tabla de matriculados";
        //                    resultado.Icono = "success";

        //                }else{
        //                    resultado.Mensaje = $"No se han encontrado cambios con los estudiantes ya registrados";
        //                    resultado.Icono = "info";
        //                }
        //                    MensajeRespuestaValidacionPermiso(resultado);
        //            }
        //        }

        //        _configServices.guardarUsuariosFromExcel();
        //        Console.WriteLine("Datos insertados en la base de datos correctamente.");
        //        Console.WriteLine($"han sido intertadas {contador} personas a la tabla de matriculados");
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ResponseModel resultado = new();
        //        resultado.Mensaje = $"No se ha suministrado ningun archivo";
        //        resultado.Icono = "info";
        //        MensajeRespuestaValidacionPermiso(resultado);
        //        Console.WriteLine("no tenemos el csv");
        //        ViewBag.MensajeError = "Por favor, seleccione un archivo CSV.";
        //        return RedirectToAction("Index");
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> SubirArchivoMartriculados(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Archivo no seleccionado o vacío.");
            }

            List<string> filasInvalidas = new List<string>();
            int contador = 0;

            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                string fila;
                while ((fila = await streamReader.ReadLineAsync()) != null)
                {
                    string[] valores = fila.Split(';'); // Supongamos que el delimitador es ','

                    if (valores.Length != 3)
                    {
                        // La fila no tiene el formato correcto
                        filasInvalidas.Add(fila);
                        continue;
                    }

                    if (long.TryParse(valores[0], out long documento))
                    {
                        Console.WriteLine("entro al si?");
                        if (await _configServices.ValidarEstudianteMatriculado(documento))
                        {
                            Console.WriteLine("No vamos a registrar el estudiante porque ya existe en la base de datos");
                        }
                        else
                        {
                            Console.WriteLine("Vamos a crear el estudiante ya que no existe en la base de datos");
                            var matriculado = new Matriculados
                            {
                                Documento = documento,
                                Nombre = valores[1],
                                Apellido = valores[2]
                            };

                            Console.WriteLine($"Se supone que creamos un matriculado con el siguiente número de documento: {matriculado.Documento}");
                            _configServices.RegistrarExcel(matriculado);

                            contador++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("hay un numero mal?");
                        // No se pudo convertir el documento a long
                        filasInvalidas.Add(fila);
                    }
                }
            }
            Console.WriteLine(filasInvalidas.Count);
            // Establecer el mensaje de alerta para mostrar en la vista Index
            if (contador > 0)
            {
                _configServices.guardarUsuariosFromExcel();
                Console.WriteLine("Datos insertados en la base de datos correctamente.");
            }

            TempData["Mensaje"] = $"Se han registrado {contador} estudiantes. {filasInvalidas.Count} filas no pudieron ser registradas.";
            TempData["Icono"] = "info";
            // Guardar las filas inválidas en un archivo CSV
            if (filasInvalidas.Count > 0)
            {
                TempData["MensajeErrorCsv"] = $"Se han registrado {contador} estudiantes. {filasInvalidas.Count} filas no pudieron ser registradas.";
                TempData["Icono"] = "danger";
                string archivoInvalido = GuardarArchivoCSVInvalido(filasInvalidas);
                byte[] archivoBytes = Encoding.UTF8.GetBytes(archivoInvalido);
                return File(archivoBytes, "text/csv", "filas_invalidas.csv");
            }
            // Construir el mensaje de respuesta
            ResponseModel resultado = new ResponseModel();
            if (contador < 0)
            {
                resultado.Mensaje = $"No se han encontrado cambios con los estudiantes ya registrados";
                resultado.Icono = "info";
                MensajeRespuestaValidacionPermiso(resultado);
            }



            return RedirectToAction(nameof(Index));
        }
        private string GuardarArchivoCSVInvalido(List<string> filasInvalidas)
        {
            StringBuilder csvBuilder = new StringBuilder();
            foreach (var fila in filasInvalidas)
            {
                Console.WriteLine("esta en el foreach de las filas invalidas");
                csvBuilder.AppendLine(fila);
            }
            return csvBuilder.ToString();
        }

        public async Task<IActionResult> UsuariosInactivos()
        {
            try
            {       

                var usuarios =  await _configServices.UsuariosInactivos()  ;
                 ViewBag.UsuariosInactivos  = usuarios.Count;
                return View(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: ConfiguracionController
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //creamos un objeto de ConfiguracionViewModel para poder enviar varios modelos a la vista
            var detalles = await _configServices.CrearViewModel();

            //iteramos sobre cada rol para mostrar las configurciones asociadas
            foreach (var rol in detalles.Roles)
            {
                //hacemos uso de la funcion mostrarConfiguracion la cual se encargara de añadir a la lista de las configuraciones del rol, las configuraciones asociadas a esta
                //debemos enviarle el id del rol por el cual va buscar y el modelo para que le añada las condfiguraciones a su respectiva lista
                detalles = _configServices.MostrarConfiguracion(rol.Id, detalles);
                switch (rol.Id)
                {
                    case 1:
                        ViewData["PermisosNoAsociadosAdmin"] = new SelectList(_configServices.PermisosNoAsociados(rol.Id), "Id", "Nombre");
                        break;
                    case 2:
                        ViewData["PermisosNoAsociadosUser"] = new SelectList(_configServices.PermisosNoAsociados(rol.Id), "Id", "Nombre");
                        break;
                    case 3:
                        ViewData["PermisosNoAsociadosAlfa"] = new SelectList(_configServices.PermisosNoAsociados(rol.Id), "Id", "Nombre");
                        break;
                }
            }
            ViewData["Id_permiso"] = new SelectList(_configServices.ObtenerPermisos(), "Id", "Nombre");
            return View(detalles);
        }

        // GET: ConfiguracionController/Create
        public ActionResult Create()
        {
            ViewData["Id_permiso"] = new SelectList(_configServices.ObtenerPermisos(), "Id", "Id");
            return View();
        }

        // POST: ConfiguracionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Configuracion configuracion)
        {
            try
            {
                await _configServices.Create(configuracion);
                TempData["Mensaje"] = $"la acción se ha realizado con exito";
                TempData["Icono"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return Json(new { success = false }); // Respuesta JSON
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                Console.WriteLine($"este es el id que esta llegando del formulario {id}");
                await _configServices.Edit(id);
                TempData["Mensaje"] = $"la acción se ha realizado con exito";
                TempData["Icono"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return Json(new { success = false }); // Respuesta JSON
            }
        }

        private void MensajeRespuestaValidacionPermiso(ResponseModel resultado){
            
            TempData["Mensaje"] = JsonConvert.SerializeObject(resultado);
            Console.WriteLine("ya pusimos el mensaje en la variable global");
             
         }       

        // POST: ConfiguracionController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _configServices.Delete(id);
            TempData["Mensaje"] = $"la acción se ha realizado con exito";
            TempData["Icono"] = "success";
            return RedirectToAction(nameof(Index));
            
        }

    }
}
//public IActionResult AccesoDenegado()
//{
//    if (User.Identity.IsAuthenticated)
//    {
//        if (User.IsInRole("1"))
//        {
//            // Usuario con rol "1" intentó acceder a una página protegida, puedes redirigirlo si lo deseas
//            return RedirectToAction(nameof(Index));
//        }
//    }

//    // Si el usuario no está autenticado o no tiene el rol "1", muestra la página de acceso denegado normalmente
//    return View();

//}