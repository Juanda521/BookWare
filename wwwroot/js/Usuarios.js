function MostrarMensajeUsuarios(icono,Mensaje) {

    Mensaje = Mensaje.replace(/^"(.*)"$/, '$1'); // Elimina las comillas al principio y al final
    Swal.fire({
        icon: icono,
        
        // title: 'BookWare Dice',
        toast : true,
        text: Mensaje,
     
        
        
        // footer: '<a>!BookWare!</a>'
    });
}

document.addEventListener('DOMContentLoaded', function() {
    const passwordInput = document.getElementById('Contraseña');
    const togglePasswordButton = document.getElementById('togglePassword');

    if (passwordInput && togglePasswordButton) {
        togglePasswordButton.addEventListener('click', function() {
            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                togglePasswordButton.querySelector('i').classList.remove('fa-eye');
                togglePasswordButton.querySelector('i').classList.add('fa-eye-slash');
            } else {
                passwordInput.type = 'password';
                togglePasswordButton.querySelector('i').classList.remove('fa-eye-slash');
                togglePasswordButton.querySelector('i').classList.add('fa-eye');
            }
        });
    }

   
});
document.addEventListener('DOMContentLoaded', function () {
    const passwordInput = document.getElementById('Contraseña');
    const togglePasswordButton = document.getElementById('togglePassword1');

    if (passwordInput && togglePasswordButton) {
        togglePasswordButton.addEventListener('click', function () {
            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                togglePasswordButton.querySelector('i').classList.remove('fa-eye');
                togglePasswordButton.querySelector('i').classList.add('fa-eye-slash');
            } else {
                passwordInput.type = 'password';
                togglePasswordButton.querySelector('i').classList.remove('fa-eye-slash');
                togglePasswordButton.querySelector('i').classList.add('fa-eye');
            }
        });
    }


});
function base64ToDataURL(base64String) {
    return "data:image/png;base64," + base64String;
}

$(document).ready(function () {
    // Escucha el evento click en los botones con la clase 'libros-relacionados-button'
    $('.libros-relacionados-button').click(function () {
        // Obtén el ID del libro desde el atributo 'data-id'
        var idLibro = $(this).data('id');
       
        // Llama a la función para obtener libros relacionados
        obtenerLibrosRelacionados(idLibro);
    });

    // Función para obtener y mostrar los libros relacionados en el modal
    function obtenerLibrosRelacionados(idLibro) {
        
        $.ajax({
            url: '/Libros/LibrosRelacionadosPorGenero',
            type: 'GET',
            data: { idLibro: idLibro },
            success: function (data) {
                var librosRelacionadosContent = $('.relaciones-' + idLibro);
                librosRelacionadosContent.empty();
               
                for (var i = 0; i < data.length; i+=6) {

                    var librosGrupo = data.slice(i, i + 6);

                    // Crear una fila para cada grupo de libros
                    var row = $('<div class="row"></div>');
                    librosGrupo.forEach(function (libro) {
                        var col = $('<div class="col-lg-2 col-md-4 col-sm-6 col-2 mb-2"></div>');
                        var libroContainer = $('<div class="relaciones-@Libro.Id"></div>');
                      

                        // Agregar la imagen debajo del nombre del libro
                        if (libro.imagenLibro) {
                            var urlDatos = base64ToDataURL(libro.imagenLibro);
                            libroContainer.append('<img class="imagen-libro-relacionado" src="' + urlDatos + '" alt="Imagen del libro relacionado" data-libro-id="' + libro.id + '">');
                        }
                        libroContainer.append('<p class="fw-bold">' + libro.nombre + '</p>');

                        col.append(libroContainer);
                        row.append(col);
                    });

                    // Agregar la fila al contenedor de libros relacionados
                    librosRelacionadosContent.append(row);
                }
               

                console.log(data)
            },
            error: function () {
                console.log('Hubo un error al obtener los libros relacionados.');
            }
        });
    }
});

$(document).on('click', '.imagen-libro-relacionado', function () {
    var libroId = $(this).data('libro-id');
    console.log('#info_' + libroId);
    $('#info_' + libroId).modal('show');
});

$(document).ready(function () {
    $('.chkCambiarUsuario').change(function () {
        var usuarioId = $(this).data('usuario-id');
        $('#usuarioId').val(usuarioId);
        console.log("ID del usuario: " + usuarioId);
        $('#formCambiarEstadoUsuario').submit();
        console.log("le dimos click al boton y enviamos el formulario de editar usuario");
    });
});



