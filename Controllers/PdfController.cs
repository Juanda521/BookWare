using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using tallerbiblioteca.Context;
using tallerbiblioteca.Services;
using PdfSharp.Fonts;
using System.Reflection;
using QuestPDF;
using QuestPDF.Fluent;
using PdfSharp.Pdf.IO;
using QuestPDF.Helpers;
using System.Drawing;
using tallerbiblioteca.Models;

namespace tallerbiblioteca.Controllers
{
    public class PdfController : Controller
    {
        private readonly BibliotecaDbContext _context;
        private readonly UsuariosServices _usuariosServices;
        private readonly PeticionesServices _peticionesServices;
        private readonly ReservasServices _reservasServices;
        private readonly PrestamosServices _prestamosServices;
        private readonly DevolucionesServices _devolucionesServices;
        private readonly SancionesServices _sancionesServices;
        private readonly LibrosServices _librosServices;
        //private readonly PdfServices _pdfServices;

        public PdfController(BibliotecaDbContext context, UsuariosServices usuariosServices, PeticionesServices peticionesServices, ReservasServices reservasServices, PrestamosServices prestamosServices, DevolucionesServices devolucionesServices, SancionesServices sancionesServices,LibrosServices librosServices)
        {
            _context = context;
            _usuariosServices = usuariosServices;
            _peticionesServices = peticionesServices;
            _reservasServices = reservasServices;
            _prestamosServices = prestamosServices;
            _devolucionesServices = devolucionesServices;
            _sancionesServices = sancionesServices;
            _librosServices = librosServices;
        }
        public async Task<IActionResult> GenerarPdfUsuarios()
        {
            var usuarios = await _usuariosServices.ObtenerUsuariosPdf();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                            

                        });

                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Usuarios registrados").FontSize(12);
                            col.Item().AlignCenter().Text("Correo: BookWare2024@gmail.com").FontSize(10);
                            col.Item().AlignCenter().Text(" Luz, Orden y Progreso ").FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Usuarios Registrados").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(13);
                            });

                            // Encabezado
                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombres").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Correo").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Rol").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Documento").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");
                            });

                            // Datos de usuarios
                            foreach (var usuario in usuarios) { 
                            
                                var nombreape = usuario.Name + " " + usuario.Apellido;
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreape).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(usuario.Correo);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(usuario.Rol.Nombre.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(usuario.Numero_documento.ToString()).FontSize(10);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(usuario.Estado).FontSize(12);
                            }
                        });
                    });
                    // Pie de página
                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Usuarios.pdf");
        }
        public async Task<IActionResult> GenerarPdfPeticiones()
        {
            var Peticiones = await _peticionesServices.ObtenerpeticionesPdf();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    // Cabecera con logo y texto personalizado
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                        });

                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Peticiones").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Correo: bookware2024@gmail.com").Bold().FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Peticiones Registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(11);
                            });

                            // Encabezado
                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Id").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Libro").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombres").Bold().FontColor("#ffffff");
                               
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Fecha").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");
                            });

                            // Datos de usuarios
                            foreach (var peticiones in Peticiones)
                            {
                                var nombreape = peticiones.Usuario.Name + " " + peticiones.Usuario.Apellido;
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(peticiones.Id.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(peticiones.Ejemplar.Libro.Nombre).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreape);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(peticiones.FechaPeticion.ToString()).FontSize(10);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(peticiones.Estado).FontSize(12);
                            }
                        });
                    });
                    // Pie de página
                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Peticiones.pdf");
        }
        public async Task<IActionResult> GenerarPdfReservas()
        {
            var reservas = await _reservasServices.ObtenerReservasPdf();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {

                   
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                        });

                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Reservas").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Correo: bookware2024@gmail.com").Bold().FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Reservas Registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                
                            });

                            // Encabezado
                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Id").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Ejemplar").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombre").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Fecha").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");
                            });

                            // Datos de usuarios
                            foreach (var reserva in reservas)
                            {
                                var nombreape = reserva.Usuario.Name + " " + reserva.Usuario.Apellido;
                                var libroId = reserva.Ejemplar.Libro.Nombre + " ISBN " + reserva.Ejemplar.Isbn_libro;
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(reserva.Id.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(libroId).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreape);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(reserva.FechaReserva.ToString()).FontSize(10);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(reserva.Estado).FontSize(12);
                            }
                        });
                    });
                    // Pie de página
                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Reservas.pdf");
        }
        public async Task<IActionResult> GenerarPdfPrestamos()
        {
            var prestamos = await _prestamosServices.ObtenerPrestamos();
            
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {

                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                        });

                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Préstamos").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Correo: bookware2024@gmail.com").Bold().FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Préstamos Registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(10);


                            });
                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Isbn").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Libro").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombre").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Prestado").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Devolución").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");

                            });

                            
                            foreach (var prestamo in prestamos)
                            {
                                var nombreCompleto = string.Concat(prestamo.Peticion.NombreUsuario, " ", prestamo.Peticion.Usuario.Apellido);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(prestamo.Peticion.Ejemplar.Isbn_libro.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(prestamo.Peticion.NombreLibro);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreCompleto).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(prestamo.Fecha_inicio.ToString()).FontSize(10);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(prestamo.Fecha_fin.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(prestamo.Estado).FontSize(12);

                            }
                        });
                    });
                    // Pie de página
                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Préstamos.pdf");
        }
        public async Task<IActionResult> GenerarPdfDevoluciones()
        {
            Console.WriteLine("LLEGANDO A HACER PDF");
            var devoluciones = await _devolucionesServices.ObtenerDevoluciones();
            
           
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);


                        });

                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Devoluciones registradas").FontSize(12);
                            col.Item().AlignCenter().Text("Correo: BookWare2024@gmail.com").FontSize(10);
                            col.Item().AlignCenter().Text(" Luz, Orden y Progreso ").FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Devoluciones registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(10);
                                columns.RelativeColumn(10);


                            });

                          
                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Id").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Libro").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombres").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Observaciones").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Fecha").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");


                            });

                         

                            foreach (var devolucion in devoluciones)
                            {
                                var nombreape = devolucion.Prestamo.Peticion.Usuario.Name + " " + devolucion.Prestamo.Peticion.Usuario.Apellido; 
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(devolucion.Id.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(devolucion.Prestamo.Peticion.Ejemplar.Libro.Nombre).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreape.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(devolucion.Observaciones).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(devolucion.Fecha_devolucion.ToString()).FontSize(10);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(devolucion.Prestamo.Peticion.Usuario.Estado).FontSize(10);


                            }
                        });
                    });
                   
                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Devoluciones.pdf");
        }
        public async Task<IActionResult> GenerarPdfSanciones()
        {
            var sanciones = await _sancionesServices.ObtenerSancionesPdf();


            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                        });
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Sanciones registradas").FontSize(12);
                            col.Item().AlignCenter().Text("Correo: BookWare2024@gmail.com").FontSize(10);
                            col.Item().AlignCenter().Text(" Luz, Orden y Progreso ").FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Sanciones registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(8);

                            });


                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Id").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombre").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Motivo").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Fecha").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Estado").Bold().FontColor("#ffffff");
                            });



                            foreach (var sancion in sanciones)
                            {
                           
                                var nombreape = sancion.Devolucion.Prestamo.Peticion.Usuario.Name + " " + sancion.Devolucion.Prestamo.Peticion.Usuario.Apellido;
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(sancion.Id.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(nombreape).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(sancion.Motivo_sancion).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(sancion.Fecha_Sancion.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(sancion.Devolucion.Prestamo.Peticion.Usuario.Estado).FontSize(12);



                            }
                        });
                    });

                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Sanciones.pdf");
        }
        public async Task<IActionResult> GenerarPdfLibros()
        {
            var Libros = await _librosServices.ObtenerLibros();


            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Escudo.png");

                        row.ConstantItem(95).Background(Colors.White).Image(rutaImagen);
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Bookware").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Institución Educativa San Lorenzo de Aburrá").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Dirección: Cr 39 #80-33").FontSize(10);
                        });
                        row.RelativeItem()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text("Libros registrados").FontSize(12);
                            col.Item().AlignCenter().Text("Correo: BookWare2024@gmail.com").FontSize(10);
                            col.Item().AlignCenter().Text(" Luz, Orden y Progreso ").FontSize(10);
                        });
                    });
                    // Cuerpo con tabla personalizada
                    page.Content().PaddingVertical(20).Column(col1 =>
                    {
                        col1.Item().AlignCenter().Text("Libros registradas").Bold().FontSize(18).FontColor("#1e6042");

                        col1.Item().PaddingVertical(15).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(15);
                                columns.RelativeColumn(8);

                            });


                            tabla.Header(header =>
                            {
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Id").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Nombre").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Cantidad").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Genero").Bold().FontColor("#ffffff");
                                header.Cell().Background("#1e6042").Border(0.5f).Padding(4).AlignCenter().Text("Autor").Bold().FontColor("#ffffff");
                            });



                            foreach (var Libr in Libros)
                            {
                                Console.WriteLine(Libr.GeneroLibros.ToString(),Libr.AutoresRelacionados);
                                
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(Libr.Id.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(Libr.Nombre).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(Libr.CantidadLibros.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(Libr.GenerosRelacionados.ToString()).FontSize(12);
                                tabla.Cell().Border(0.5f).Padding(2).AlignCenter().Text(Libr.AutorLibros.ToString()).FontSize(12);



                            }
                        });
                    });

                    page.Footer().Height(65)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Página ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                });
            })
            .GeneratePdf();

            Stream stream = new MemoryStream(pdf);

            return File(stream, "application/pdf", "Libros.pdf");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}