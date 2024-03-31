using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace tallerbiblioteca.Models
{
    public class Sancion

    {
        public int Id { get; set; }

        [ForeignKey("Devolucion")]
        public int Id_devolucion { get; set; }
        public Devolucion Devolucion { get; set; } = new();

        [Required(ErrorMessage = "El motivo de la sancion es obligatoria")]
        public string Motivo_sancion { get; set; } = "";

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "La fecha es obligatoria")]

        public DateTime Fecha_Sancion { get; set; }

        public string Estado {get;set;}
        // public virtual List<Sancion> sanciones;
        

    }
}
