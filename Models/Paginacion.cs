
namespace tallerbiblioteca.Models{


    public class Paginacion<T>{
        //items con los que va trabajar, ya sea usuarios, libros, prestamos etc...
        public List<T> Items { get; }
        //cantidad total de registros en la base de datos de la tabla a mostrar
        public int TotalItems { get; }
        
        public int Numero_pagina { get; }
        //cantidad de items que van aparecer en la tabla
        public int ItemsPagina { get; }

        public LibroViewModel? LibroViewModel {get; set;} = new();

        public CatalogoViewModel? CatalogoViewModel {get;set;}

        public PeticionesViewModel? PeticionesViewModel { get; set; } = new();

        public List<Sancion>? Sanciones {get; set;} = new();

        
        public Paginacion(List<T> items, int totalItems, int numero_pagina, int itemsPagina)
        {
            Items  = items;
            TotalItems = totalItems;
            Numero_pagina = numero_pagina;
            ItemsPagina = itemsPagina;
        }

        public static implicit operator Paginacion<T>(Paginacion<Publicaciones> v)
        {
            throw new NotImplementedException();
        }
    }
}