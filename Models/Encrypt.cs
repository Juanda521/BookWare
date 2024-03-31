namespace tallerbiblioteca.Models
{
    public class Encrypt{


        public string Encryptar( string cadena_encriptar){
            
            
            string resultado;
            byte[] encrypt =  System.Text.Encoding.Unicode.GetBytes(cadena_encriptar);

            resultado = Convert.ToBase64String(encrypt);

            return resultado;
        }
        public string Desencriptar(string cadena_encriptada)
        {
            string resultado;
            byte[] decrypt = Convert.FromBase64String(cadena_encriptada);

            resultado = System.Text.Encoding.UTF8.GetString(decrypt);

            return resultado;
        }
    }
}