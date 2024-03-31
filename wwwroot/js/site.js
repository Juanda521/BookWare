// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
 




document.addEventListener("DOMContentLoaded", function () {
    // Hacer una solicitud a la API para obtener los datos
    fetch("/api/Notificaciones")
        .then(response => response.json()) // Suponiendo que los datos son un JSON
        .then(data => {
         

            // Actualizar el contenido de la campana con los datos obtenidos
            document.querySelector("#valorCampana").textContent = data.length;

            // Actualizar el dropdown con las notificaciones
            const notificacionesContainer = document.querySelector("#notificacionesContainer");

            if (data.length > 0) {
                data.forEach(item => {
                    const notificacion = document.createElement("li");
                    const enlace = document.createElement("a");
                    const hr = document.createElement("hr"); // Crear línea horizontal
                    if(item.estado != "ACEPTADA" && item.estado != "RECHAZADA"){
                        if (item.usuario && item.usuario.name) {
                            enlace.textContent = `${item.usuario.name} ha solicitado. ${item.motivo}`;
                        } else {
                            enlace.textContent = `Alguien ha solicitado. ${item.motivo}`;
                        }
                        // Agregar evento de clic al enlace
                            enlace.addEventListener("click", () => {
                                // Aquí colocas la lógica de redirección
                                // Por ejemplo:
                                window.location.href = "https://localhost:7067/Peticiones/Index";
                            });

                            // Agregar el enlace al elemento li
                            notificacion.appendChild(enlace);
                            notificacion.appendChild(hr);
                  
                    }else{
                        console.log("la peticion ya se ha aceptado");
                        notificacionesContainer.innerHTML = "<li>No hay notificaciones nuevas.</li>";
                    }
                   
                    notificacionesContainer.appendChild(notificacion);
                   
                });
                // Cambiar el color del ícono
                document.querySelector("#valorCampana").style.color = "white";
                document.querySelector("#valorCampana").style.background = "black";
            } else {
                // No hay notificaciones nuevas, puedes ocultar el contenedor o mostrar un mensaje predeterminado
                notificacionesContainer.innerHTML = "<li>No hay notificaciones nuevas.</li>";
            }

            // Hacer algo con los datos recibidos si es necesario
        })
        .catch(error => console.error(error));
});






