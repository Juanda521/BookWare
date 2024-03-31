using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;


using tallerbiblioteca.Services;

public class BackupController : Controller
{
    // private readonly string _connectionString; // Obtén la cadena de conexión desde tu appsettings.json

    private readonly BackupService _backupService;



    public BackupController(BackupService backupService)
    {
       _backupService = backupService;
    }


    public async Task<IActionResult> DownloadBackup(bool? descarga = false)
    {
        Console.WriteLine("Estamos en la funcion de descargar el backup");

        if (descarga.GetValueOrDefault())
        {
            string BackupFileName = await _backupService.GenerateDatabaseBackupAsync();

            // ... el resto del código para enviar el archivo de respaldo al cliente ...
            var backupPath = Path.Combine(await _backupService.ObtenerRuta(), BackupFileName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(backupPath, FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            TempData["descargaFinalizada"] = true;
            Console.WriteLine("VAMOS A VEOLVER LA DESCARGA Y EL TEMPDATA ESTA EN TURE");
            
            return File(memoryStream, "application/octet-stream", "BibliotecaFinal.bak");
        }
            else{
        return RedirectToAction("Index","Configuracion");
        }
    }



// [HttpPost]
// public IActionResult BackupGeneral()
// {

//     Console.WriteLine("estamos en la funcion del backup");
//     try
//     {
//         using (SqlConnection connection = new SqlConnection(_connectionString))
//         {
//             connection.Open();
//             Console.WriteLine("tenemos abierta la conexion");

//             // Nombre de la base de datos que deseas respaldar
//             string databaseName = "BibliotecaFinal";

//             // Crear un nombre de archivo único para la copia de seguridad
//             string backupFileName = $"{databaseName}_Backup_{DateTime.Now:yyyyMMddHHmmss}.bak";

//             // Ruta temporal donde se guardará la copia de seguridad
//             string tempBackupPath = Path.Combine(Path.GetTempPath(), backupFileName);

//             // Comando T-SQL para realizar la copia de seguridad
//             string backupCommand = $"BACKUP DATABASE {databaseName} TO DISK = '{tempBackupPath}'";

//             Console.WriteLine($"tenemos el comando para realizar el backup {backupCommand}");

//              Console.WriteLine($"tenemos el la conexion para realizar el backup {connection}");

//              try
//              {
//                 using (SqlCommand command = new SqlCommand(backupCommand, connection))
//                     {
//                         command.ExecuteNonQuery();
//                         Console.WriteLine("se supone que hemos hecho la copia de seguridad");
//                         Console.Out.Flush(); // Agrega esta línea
//                     }
//              }
//                 catch (SqlException sqlEx)
//             {
//                 // Manejar excepciones específicas de SQL
//                var message = $"Error SQL al realizar la copia de seguridad: {sqlEx.Message}";
//                 ViewBag.Error = message;
//             }
//             catch (Exception ex)
//             {
//                 // Manejar otras excepciones generales
//                var  message = $"Error al realizar la copia de seguridad: {ex.Message}";
//                 ViewBag.Error = message;
//             }
//             Console.WriteLine("no hizo la copia de seguridad?");
//             Console.Out.Flush(); // Agrega esta línea
//             // Devolver la copia de seguridad como una descarga al usuario
//             byte[] fileBytes = System.IO.File.ReadAllBytes(tempBackupPath);
//             return File(fileBytes, "application/octet-stream", backupFileName);
//         }
//     }
//     catch (Exception ex)
//     {
//         ViewBag.Error = $"Error al realizar la copia de seguridad: {ex.Message}";
//     }
//     Console.WriteLine("vamos a retornar ");
//     return RedirectToAction("Index","Libros");
// }

}