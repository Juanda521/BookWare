

function MensajeBackup(descargaFinalizada){
    if (descargaFinalizada != null && descargaFinalizada.toLowerCase() === "true") {
        Console.WriteLine("llego el viewdata");
        alert("¡Descarga finalizada!");
    }else{
        Console.log("no esta llegando el viewdata");
    }
}

$(document).ready(function() {
    $('.chkCambiarPermiso').change(function() {
        var configId = $(this).data('config-id');
        $('#configId').val(configId);
        console.log("ID de la configuracion: " + configId);
        $('#formCambiarCampoPermiso').submit();
        console.log("le dimos click al boton y enviamos el formulario");
    });
});



function sendForm(e) {
    e.preventDefault();
    Swal.fire({
        title: 'Quieres guardar los cambios?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'SI'
    }).then((resultado) => {
        if (resultado.isConfirmed) {
            const formulario = document.getElementById("formulario")
            formulario.submit();
        }
    })
}

function sendFormBackup(e) {
    const fechaActual = new Date(); 
    e.preventDefault();
    Swal.fire({
        title: 'Quieres guardar la copia de seguridad de los cambios hechos hasta:' + fechaActual +'?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'SI'
    }).then((resultado) => {
        if (resultado.isConfirmed) {
            const formulario = document.getElementById("formBackup")
            console.log("vamos a enviar el formulario de realizar backup");
            formulario.submit();
        }
    })
}



function sendFormDelete(e,idConfig) {
    e.preventDefault();
    Swal.fire({
        title: 'Seguro que deseas eliminar esta configuracion?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'SI'
    }).then((resultado) => {
        if (resultado.isConfirmed) {
            const formulario = document.getElementById("form_"+idConfig)
            console.log(formulario)
            formulario.submit();
        }
    })
}




//function cambiarEstadoChecbox() {

//    const checkboxes = document.querySelectorAll('.checkboxEstadoPermiso');
//    checkboxes.forEach(checkbox => {
//        if (checkbox.value === "ACTIVO") {
//            checkbox.checked = true;
//        } else {
//            checkbox.checked = false;
//            //console.log("esta desactivado");
//        }
//        checkbox.addEventListener('change', function () {
           

//            const checkedTrue = checkbox.checked;
         
//            const idConfig = checkbox.getAttribute('id');

//            cambiarValorDataBase(idConfig, checkedTrue);
//        })

       
//    })

//}


//function cambiarValorDataBase(idConfig, checkedTrue) {
//    var funcion;
//    if (checkedTrue) {
//        funcion = 1;
//    } else {
//        funcion = 2
//    }
//    const id = parseInt(idConfig, 10);
//    console.log("estamos en la funcion")

   
//    const data = {
//        id: id,
//        opc: funcion
//    }
//    console.log(data);

//    fetch('/Configuracion/Edit', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json'
//        },
//        body: JSON.stringify(data)
//    }).then(response => response.json())
//        .then(result => {
//            console.log("mera falla");
//            console.log(result); // Respuesta del servidor
//            // Puedes redirigir o hacer otras acciones aquí
//        })
//        .catch(error => console.error('Error:', error), console.log("mera falla"));
    


//}
