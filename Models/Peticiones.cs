

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tallerbiblioteca.Models
{
    public class Peticiones
    {
        // [Key]
        // public int idPeticion { get; set; }
        public int Id {get; set;}
        
        [ForeignKey("Ejemplar")]
        public int Id_ejemplar { get; set; }
        public Ejemplar Ejemplar {get;set;} = new ();

        [ForeignKey("Usuario")]
        public int Id_usuario { get; set; }
        public Usuario Usuario {get;set;} = new();

        public DateTime FechaPeticion { get; set; }
        [Required(ErrorMessage = "este campo es obligatorio.")]
        public string Motivo { get; set; }  = "Breve descripcion de la peticion";

        public string Estado  {get; set;} = "EN ESPERA";

          [NotMapped]
 
        public string NombreUsuario => Usuario.Name;
        public string apellido => Usuario.Apellido;
        public string NombreLibro => Ejemplar.Libro.Nombre;
        public string IdEjemplarAsString{ 
            get { return Id_ejemplar.ToString(); }
        }
        public string IdUsuarioAsString
        {
            get { return Id_usuario.ToString();}
        }
        
    

      
    }
    public class PeticionesModel
    {
        public Paginacion<Peticiones> Paginacion { get; set; }
        public Peticiones Peticiones { get; set; }

        public PeticionesModel(Paginacion<Peticiones> paginacion, Peticiones peticiones)
        {
            Paginacion = paginacion;
            this.Peticiones = peticiones;
        }
    }
}
