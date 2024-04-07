using tallerbiblioteca.Models;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace tallerbiblioteca.Services
{
    public class EmailServices:IEmailServices
    {

        private readonly IConfiguration _config;
        public EmailServices(IConfiguration config)
        {
            _config =  config;
        }

      
        public void SendEmail(SendEmailDTO correo)
        {
           
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(correo.Para));
            email.Subject = correo.Asunto;
            email.Body = new TextPart(TextFormat.Html){

                Text  = correo.Contenido
            };

            using var smtp = new SmtpClient();
            smtp.Connect(
                _config.GetSection("Email:Host").Value,
               Convert.ToInt32(_config.GetSection("Email:Port").Value),
               SecureSocketOptions.StartTls
            );

            smtp.Authenticate( _config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
              Console.WriteLine("vamos a enviar el correo");
            smtp.Send(email);
              Console.WriteLine("enviamos  el correo");
            smtp.Disconnect(true);
        }

        public SendEmailDTO EmailRegisterUser(string destinatario){
            Console.WriteLine("vamos a preparar el correo");
             Console.WriteLine(destinatario);
            return new(){
                Para  = destinatario,
                Asunto = "Registro a Aplicatívo Bookware",
                Contenido = "Te has registrado a nuestro aplicatívo con el fin de hacer uso de nuestros servicios como lo pueden ser prestamos y reservas del material biliográfico de la institución. \n Te agradezemos el utilizar nuestro servicio y esperamos que disfrutes todo lo que te podemos brindar"
            };
        }

        public SendEmailDTO EmailPrestamo(Prestamo prestamo){
            Console.WriteLine("este es el id del prestamo:" +prestamo.Id);
            return new(){
                Para  = prestamo.Peticion.Usuario.Correo,
                Asunto = "Petición Confirmada",
                Contenido = $"Tu Petición por préstamo del libro: {prestamo.Peticion.Ejemplar.Libro.Nombre}  ha sido aceptada, puedes acercarte a la institución por el libro solicitado. \n Recuerda devolver el libro en la fecha siguiente:  {prestamo.Fecha_fin} para evitar ser sancionado. \n Atentamente: BookWare :)! \n Disfrutalo "
    
            };
        }

        public SendEmailDTO EmailPeticion(Peticiones peticion){
            Console.WriteLine("VAMOS A ENVIAR UN CORREO A "+peticion.Usuario.Correo);
            return new(){
                Para  = "Bookware2024@gmail.com",
                Asunto = "Petición de Préstamo de Libro",
                Contenido = "La persona "+ peticion.Usuario.Name + " ha solicitado el prestamo del libro "+peticion.Ejemplar.Libro.Nombre +" ingresa al aplicatívo para obtener más información y poder aceptar o rechazar la petición"
            };
        }

        public SendEmailDTO EmailRecuperarContraseña(Usuario usuario,int Codigo)
        {
            return new()
            {
                Para = usuario.Correo,
                Asunto = "Recuperación de contraseña",
                Contenido = $@"Has solicitado el cambio de tu contraseña. el siguiente codigo te permitira
                restablecer tu contraseña
                <span style=""font-size: 24px; display: block; text-align: center; margin-top: 10px"">
                {Codigo}</span>"
            };
        }

    }
}
