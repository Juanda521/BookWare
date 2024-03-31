using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tallerbiblioteca.Models

{
    public class Libro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string Nombre { get; set; } ="";

        public int CantidadLibros { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string Descripcion{ get; set; }  ="";

        public string ImagenLibro{get; set;} = "";

        public string Estado {get; set;}  = "ACTIVO";

        [NotMapped]
        public List<int> GeneroIds { get; set; }
        [NotMapped]
        public List<int> AutorIds { get; set; }

        [NotMapped]
        public List <Autor> Autores {get; set;}

        [NotMapped]
        public List <Genero> Generos {get; set;}

        [NotMapped]
        public List <Ejemplar> Ejemplares {get; set;}

        [NotMapped]
        public List<AutorLibro> AutoresRelacionados  {get; set;} = new();

        [NotMapped]
        public List<GeneroLibro> GenerosRelacionados  {get; set;}  = new();

        public ICollection<GeneroLibro> GeneroLibros { get; set; }

        public ICollection<AutorLibro> AutorLibros { get; set; }

       
    }
}
