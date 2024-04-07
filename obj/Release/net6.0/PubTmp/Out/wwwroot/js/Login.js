$(document).ready(function() {
    $('#loginForm').submit(function(event) {
        event.preventDefault(); // Evitar que se envíe el formulario de forma predeterminada

        var numeroDocumento = $('#numeroDocumento').val();
        var contraseña = $('#contraseña').val();
        var errores = false;

        // Verificar que los campos no estén vacíos
        if (numeroDocumento.trim() === '') {
            $('#numeroDocumentoError').text('Por favor, complete este campo.');
            errores = true;
        } else {
            $('#numeroDocumentoError').text('');
        }

       // Validar contraseña
        if (contraseña.trim() === '') {
            $('#contraseñaError').text('Por favor, complete este campo.');
            errores = true;
        } else if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}/.test(contraseña)) {
            $('#contraseñaError').text('La contraseña debe contener al menos una mayúscula, una minúscula, un número y 8 caracteres.');
            errores = true;
        } else {
            $('#contraseñaError').text('');
        }

        // Si hay errores, no enviar el formulario
        if (errores) {
            return;
        }

        // Enviar datos del formulario utilizando AJAX
        $.ajax({
            url: '/Usuarios/Login', // URL de tu controlador ASP.NET Core
            type: 'POST',
            data: {
                Numero_documento: numeroDocumento,
                Contraseña: contraseña
            },
            success: function(response) {
                // Manejar la respuesta del servidor
                console.log(response);
                window.location.href = '/Libros/Catalog';
            },
            error: function(xhr, status, error) {
                // Manejar errores de la solicitud AJAX
                console.error(error);
                  var mensajeError = '';
                if (xhr.status === 400) {
                    mensajeError = 'Credenciales incorrectas.';
                } else if (xhr.status === 404) {
                    mensajeError = 'Usuario no encontrado.';
                }else if(xhr.status === 403){
                    mensajeError  ='Te encuentras suspendido, no puedes acceder al aplicativo';
                }
                $('#errorSpan').text(mensajeError);
            }
        });
    });
});