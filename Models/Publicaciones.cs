using MessagePack;
using System.ComponentModel.DataAnnotations;
using tallerbiblioteca.Controllers;

namespace tallerbiblioteca.Models
{
    public class Publicaciones
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "este campo es obligatorio.")]
        public string Tipo { get; set; }
        [Required(ErrorMessage = "El tipo de publicación es obligatorio.")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El nombre de la publicación es obligatorio.")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "La descripción de la publicación  es obligatorio.")]
        public DateTime FechaInicio { get; set; }
        [Required(ErrorMessage = "este campo es obligatorio.")]
 
        public DateTime FechaFin { get; set; }

        public string Estado { get; set; }
        [Required(ErrorMessage = "este campo es obligatorio.")]
        public byte[] Imagen { get; set; }
    }
    public class PublicacionesModel
    {
        public Paginacion<Publicaciones> Paginacion { get; set; }
        public Publicaciones Publicacion { get; set; }

        public PublicacionesModel(Paginacion<Publicaciones> paginacion, Publicaciones publicaciones)
        {
            Paginacion = paginacion;
            this.Publicacion = publicaciones;
        }
    }
}