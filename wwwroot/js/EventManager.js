
document.addEventListener('DOMContentLoaded', function() {
  var calendarEl = document.getElementById('calendar');
  var allEvents = []; // Array para almacenar todos los eventos


  //para realizar la grafica
  fetch("/api/Graficas")
    .then(response => response.json()) // Suponiendo que los datos son un JSON
    .then(prestamos => {
      console.log("estamos con los datos");
      console.log(prestamos);
      var ctx = document.getElementById('graficaLibros').getContext('2d');

      var colors = ['#1e6042', 'rgb(0, 0, 0)','#1e6042'];

      var data = {
        labels: prestamos.map(prestamo=>prestamo.libro),
        datasets: [{
          label: 'Prestamos por Libros',
          data: prestamos.map(prestamo=>prestamo.cantidad),
          backgroundColor: colors,
        }]
      };

      var options = {
        responsive: true,
        maintainAspectRatio: false,
        indexAxis : 'y',
        cutout: '40%', // Controla el tamaño del agujero en el centro del gráfico (70% en este caso)
        barThickness: 30 ,
      };


      var bookChart = new Chart(ctx, {
          type: 'bar',
          data: data,
          options: options
      });
  
    }).catch(error => {
      console.error('Error al procesar la solicitud:', error);
      // Aquí puedes manejar el error, como mostrar un mensaje al usuario o registrar el error
    });
    

  function handleDatesRender(info) {
    console.log('viewType:', info.view.type);
  }

      // Función para abrir el modal Bootstrap
    function openBootstrapModal(eventstr,userstr,librostr,idUser,fechafin,estado) {
      // Obtener el elemento del modal de fecha
      try{
        const modalEvent = document.getElementById('modal-event');
        const modalUser  = document.getElementById('modal-user');
        const modalLibro = document.getElementById('modal-libro');
        const modalCc = document.getElementById('modal-id-usuario');
        const modal_fecha_fin = document.getElementById('modal-fecha-fin');
        const modal_estado = document.getElementById('modal-estado');

        // Establecer el texto del modal de fecha
        
        modalEvent.value = eventstr;
        modalUser.value  = userstr;
        modalLibro.value = librostr;
        modalCc.value = idUser;
        modal_fecha_fin.value = fechafin;
        modal_estado.value  = estado;
        
        // Abrir el modal Bootstrap
        $('#myModal').modal('show');
      } catch (error) {
        console.error('Error al abrir el modal:', error);
        // Aquí puedes manejar el error, como mostrar un mensaje al usuario o registrar el error
      }
    }




  fetch("/api/Calendario")
    .then(response => response.json()) // Suponiendo que los datos son un JSON
    .then(data => {
        
      // Guardar los eventos de la petición en la matriz
      allEvents = data.map(function(event) {
      
        return {
          title: event.peticion.nombreUsuario,
          start: event.fecha_inicio,
          id: event.id,
          color: 'green',
          motivo : event.peticion.motivo,
          estado: event.estado,
          user: event.peticion.nombreUsuario +' ' + event.peticion.apellido,
          libro: event.peticion.nombreLibro,
          document:event.peticion.usuario.numero_documento,
          fecha_fin : event.fecha_fin
          // Puedes agregar más propiedades aquí si es necesario
        };
      });
      console.log("Todos los eventos:", allEvents);
      console.log(data);

      // Obtén el elemento de entrada y el calendario
      const searchInput = document.getElementById('searchInput');

      // Maneja el evento de entrada para filtrar los eventos del calendario
        function filtrarEventos() {
          const searchText = searchInput.value.toLowerCase();
          console.log(searchText);
        
          const filteredEvents = allEvents.filter(event => {
            const documentoVisible = event.document.toString().includes(searchText);
            return documentoVisible;
          });
      
          // Escribe los eventos filtrados en la consola
          console.log("Eventos filtrados por número de documento:", filteredEvents);

          // Actualiza el array 'allEvents' con los eventos filtrados
          calendar.removeAllEvents();
          calendar.addEventSource(filteredEvents);
          calendar.refetchEvents();
      };

        // Maneja el evento de entrada para filtrar los eventos del calendario
        searchInput.addEventListener('input', function() {
          filtrarEventos();
        });

      // Crear una variable para almacenar el elemento del calendario
      var calendar = new FullCalendar.Calendar(calendarEl, {
        
        initialView: 'dayGridMonth',
        eventColor: 'green',
        events: allEvents, // Usar la matriz de eventos combinados
        eventClick: function(info) {
          // Obtén el evento específico que se hizo clic
          const clickedEvent = info.event;
          console.log(clickedEvent);
          // Obtén la información del evento
          const motivo = clickedEvent.extendedProps.motivo;
          const estado =  clickedEvent.extendedProps.estado;
          const user = clickedEvent.extendedProps.user; // Ajusta esto según la estructura real de tu objeto de evento
          const libro = clickedEvent.extendedProps.libro; // Ajusta esto según la estructura real de tu objeto de evento
          const documento = clickedEvent.extendedProps.document; // Ajusta esto según la estructura real de tu objeto de evento
          const fechaDevolucion = clickedEvent.extendedProps.fecha_fin;
          // Abrir el modal Bootstrap y pasar las variables motivo y user
          openBootstrapModal(motivo, user, libro, documento, fechaDevolucion,estado);
        },
        datesSet: handleDatesRender,
      
        locale: 'es', 
      });
      calendar.updateSize() // Forza al calendario a ajustar su tamaño inmediatamente
      calendar.render();
    });
  });

