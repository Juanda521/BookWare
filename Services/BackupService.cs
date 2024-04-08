using System;
using System.Data.SqlClient;

namespace tallerbiblioteca.Services
{

    public class BackupService
    {
        private readonly IConfiguration _configuration;

        public BackupService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateDatabaseBackupAsync()
        {

            Console.WriteLine("estamos en los servicios del backup");
            string backupFolderPath = _configuration["BackupFolderPath"];
            
            string backupFileName = "BibliotecaFinalBookware.bak";
            string backupPath = Path.Combine(backupFolderPath, backupFileName);
            Console.WriteLine($"esta es la ruta donde guardaremos el archivo: {backupFolderPath}");

            using (var connection = new SqlConnection(_configuration.GetConnectionString("Connection")))
            {
                await connection.OpenAsync();
                Console.WriteLine("abrimos la conexion");


                var commandText = $"BACKUP DATABASE db_aa7214_bookwaresena TO DISK = '{backupPath}'";
                using (var command = new SqlCommand(commandText, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("realizamos la copia");
                }
            }
            return backupFileName;
        }

        public async Task<string> ObtenerRuta(){
           return _configuration["BackupFolderPath"];
        }
    }
}
