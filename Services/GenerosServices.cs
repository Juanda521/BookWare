using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using tallerbiblioteca.Context;
using tallerbiblioteca.Models;
using tallerbiblioteca.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Win32;
using tallerbiblioteca.Context;
using System.Globalization;
using tallerbiblioteca.Migrations;

namespace tallerbiblioteca.Services
{
    public class GenerosServices
    {
        private readonly BibliotecaDbContext _context;
        private int Status;
        private ConfiguracionServices _configuracionServices;
        private LibrosServices _librosServices;

        public GenerosServices(BibliotecaDbContext bibliotecaDbContext, ConfiguracionServices configuracionServices, LibrosServices librosServices)
        {

            _context = bibliotecaDbContext;
            _configuracionServices = configuracionServices;
            _librosServices = librosServices;
        }

        public async Task<Genero> Buscar(int Id)
        {
            var genero = await _context.Genero.Where(g => g.Id == Id ).FirstOrDefaultAsync();
            if (genero != null)
            {
                return genero;
            }
            return new();
        }

       

        // public async Task<Devolucion> BarraBusqueda(int busqueda)
        // {
        //     if(busqueda!=null)
        //     {
        //         busqueda=await _context.Devoluciones.Where(d=>d.Id.Containts(busqueda)).ToListAsync();
        //     }
        // }

        public async Task<int> Registrar(Genero genero, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Registrar_Genero", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {
                Console.WriteLine("Hola desde los services");
                genero.Estado="ACTIVO";
                
                _context.Add(genero);
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;


        }

        public List<Genero>busqueda(string busqueda ){
            return  _context.Genero.Where(l=>l.NombreGenero.ToLower().Contains(busqueda) || l.Estado.ToLower().Contains(busqueda)  ).ToList();
        }

        public async Task<List<Genero>>ObtenerGeneros(){
            return await _context.Genero.ToListAsync();
        }

        public async Task<int> Editar(Genero genero, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Genero", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {
                genero.Estado = "ACTIVO";
                _context.Update(genero);
                await _context.SaveChangesAsync();
                // return View(devolucion);

            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;

        }

        public async Task<int> Eliminar(int id, ClaimsPrincipal User)
        {

            int Status = _configuracionServices.ValidacionConfiguracionActiva("Eliminar_Genero", _configuracionServices.ObtenerRolUserOnline(User));


            if (Status == 200)
            {
                
                var genero = await _context.Genero.FindAsync(id);
                _context.Genero.Remove(genero);
                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("A mirar que esta fallando :(" + Status);

            }
            return Status;
        }

        public ResponseModel MensajeRespuestaValidacionPermiso(int Status)
        {
            return _configuracionServices.MensajeRespuestaValidacionPermiso(Status);
        }

         public int Desactivar(int id, ClaimsPrincipal User)
        {
            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;

            int Id_rol = Int32.Parse(Id_rol_string);
            //debe ser el mismo  nombre de la tabla permisos
            this.Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Genero", Id_rol);
            //si el estado que nos devolvio la validacion de la accion a realizar es correcta (status 200) podremos realizar la accion
            var genero = _context.Genero.Find(id);
            if (Status == 200)
            {
                if (genero != null)
                {
                   
                    genero.Estado = "INHABILITADO";
                    _context.SaveChanges();
                }
            }
            return Status;

        }

         public int Activar(int id, ClaimsPrincipal User)
        {
            var Id_rol_string = User.FindFirst(ClaimTypes.Role)?.Value;

            int Id_rol = Int32.Parse(Id_rol_string);
            //debe ser el mismo  nombre de la tabla permisos
            this.Status = _configuracionServices.ValidacionConfiguracionActiva("Actualizar_Genero", Id_rol);
            //si el estado que nos devolvio la validacion de la accion a realizar es correcta (status 200) podremos realizar la accion
            var genero = _context.Genero.Find(id);
            if (Status == 200)
            {
                if (genero != null)
                {
                    genero.Estado = "ACTIVO";
                    _context.SaveChanges();
                }
            }
            return Status;

        }


        


    }
}


