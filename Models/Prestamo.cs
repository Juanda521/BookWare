using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tallerbiblioteca.Models
{
    public class Prestamo
    {
        public int Id { get; set; }
        

        public int Id_peticion { get; set; } 

       [ForeignKey("Id_peticion")]
        public Peticiones Peticion { get; set; } = new();

        // [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime Fecha_inicio{ get; set; }

        public DateTime Fecha_fin { get; set; }

        public string Estado {get; set;} = "EN CURSO";
    }
}
