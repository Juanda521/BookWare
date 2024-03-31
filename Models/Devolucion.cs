using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace tallerbiblioteca.Models
{
    public class Devolucion
    {
        public int Id { get; set; }

        [ForeignKey("Prestamo")]
        public int Id_prestamo { get; set; }
        public Prestamo Prestamo { get; set; } = new();

        [Required(ErrorMessage = "La observacion es obligatoria")]
        public string Observaciones { get; set; } = "";

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha_devolucion { get; set; }
        public string Estado { get; set;  }
    }
}
