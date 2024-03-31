
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tallerbiblioteca.Models
{
    public class Autor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string NombreAutor { get; set; } = "";

        public string Estado {get; set;}  ="";

        public string Nacionalidad {get; set;} = "";

    

        [NotMapped]
        public List<Libro> Libros {get; set;} = new();
    }
}
