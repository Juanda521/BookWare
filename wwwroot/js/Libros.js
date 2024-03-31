
//validacion del formulario de registrar libros (gracias a las facilidades de asp, en este script solo se encuentra la validacion de generos y autores)

function validarYEnviarFormulario() {
    // Obtén los valores seleccionados en los selectores de género y autor
    var generoIds = document.getElementById('GeneroIds').value;
    var autorIds = document.getElementById('AutorIds').value;

    var Nombre = document.getElementById('Nombre').value;
    var Descripcion = document.getElementById('Descripcion').value;


    console.log("hablalo desde la validacion");

    // Verifica si al menos un género y un autor han sido seleccionados
    if (generoIds.length === 0 || autorIds.length === 0) {
        // Muestra un mensaje de error
        console.log("no hay generos ni autores");
        var errorSpan = document.getElementById('errorMensaje');
        if (!errorSpan) {
            console.log("vamos a crear el span");
            errorSpan = document.createElement('span');
            errorSpan.id = 'errorMensaje';
            errorSpan.style.color = 'red';
            errorSpan.innerHTML = 'Debes seleccionar al menos 1 género y 1 autor.';
            document.querySelector('#errorGenero').appendChild(errorSpan);
        }
        // Evita que el formulario se envíe si no se cumplen los requisitos
        return false;
    }else if(Nombre.length === 0  ){

        console.log("el campo nombre esta vacio, no se puede enviar el formulario");
        var errorSpan = document.getElementById('errorMensaje');
        if (errorSpan) {
            errorSpan.parentNode.removeChild(errorSpan);
        }
        return false;
    }else if(Descripcion.length ===0){
        console.log("el campo Descripcion esta vacio, no se puede enviar el formulario");
        var errorSpan = document.getElementById('errorMensaje');
        if (errorSpan) {
            errorSpan.parentNode.removeChild(errorSpan);
        }
        return false;
    }else{
        console.log("todos los campos estan llenos y procedemos a enviar el formulario");
        // Si se cumplen los requisitos, obténemos el formulario y lo enviamos
        var formulario = document.getElementById('formularioLibros');
        formulario.submit();

        // Si llegamos a este punto, eliminamos el mensaje de error si existe
        var errorSpan = document.getElementById('errorMensaje');
        if (errorSpan) {
            errorSpan.parentNode.removeChild(errorSpan);
        }
    }

    
}

$(document).ready(function() {
    $('.chkCambiarCampo').change(function() {
        var libroId = $(this).data('libro-id');
        $('#libroId').val(libroId);
        console.log("ID del libro: " + libroId);
        $('#formCambiarCampo').submit();
        console.log("le dimos click al boton y enviamos el formulario de editar libro");
    });
});

$(document).ready(function() {
    $('.chkCambiarEjemplar').change(function() {
        var ejemplarId = $(this).data('ejemplar-id');
        $('#ejemplarId').val(ejemplarId);
        console.log("ID del ejemplar: " + ejemplarId);
        $('#formCambiarEstadoEjemplar').submit();
        console.log("le dimos click al boton y enviamos el formulario de editar ejemplar");
    });
});
document.getElementById('botonRegistrar').addEventListener('click', function () {
    console.log("le dimos click a registrar libro");
    validarYEnviarFormulario();
});