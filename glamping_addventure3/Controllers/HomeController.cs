using System.Diagnostics;
using System.Security.Claims;
using glamping_addventure3.Models;
using glamping_addventure3.Models.View;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace glamping_addventure3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private GlampingAddventure3Context _dbcontext;

        public HomeController(GlampingAddventure3Context context)
        {
            _dbcontext = context;
        }

        public IActionResult resumenReserva()
        {
            try
            {
                DateTime FechaInicio = DateTime.Now.AddDays(-5);


                var Lista = (from data in _dbcontext.Reservas
                             where data.FechaReserva.HasValue && data.FechaReserva >= FechaInicio
                             group data by data.FechaReserva.Value.Date into gr
                             select new VMReserva
                             {
                                 FechaReserva = gr.Key, // Esto es de tipo DateTime?
                                 FechaReservaFormatted = gr.Key.ToString("yyyy-MM-dd"),
                                 Cantidad = gr.Count()
                             }).ToList();

                if (Lista == null || !Lista.Any())
                {
                    return NotFound("No se encontraron datos para las reservas.");
                }

                return Ok(Lista);

            }
            catch (Exception ex)
            {
                // Manejo de errores (puedes loguear el error si es necesario)
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }

        }

        public IActionResult resumenPaquete()
        {

            try
            {
                var lista = (from data in _dbcontext.DetalleReservaPaquetes
                             join tbpaquete in _dbcontext.Paquetes on data.Idpaquete equals tbpaquete.Idpaquete
                             group data by new { data.Idpaquete, tbpaquete.NombrePaquete } into gr
                             select new VMPaquete
                             {
                                 NombrePaquete = gr.Key.NombrePaquete ?? "Sin Nombre", // Manejo de nulos
                                 Cantidad = gr.Count()
                             })
                            .OrderByDescending(gr => gr.Cantidad) // Opcional: ordenar por cantidad
                            .Take(5)
                            .ToList();

                if (lista == null || !lista.Any())
                {
                    return NotFound("No se encontraron datos para las reservas.");
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }
        }

        public IActionResult resumenServicios()
        {
            try
            {
                var Lista = (from data in _dbcontext.DetalleReservaServicios
                             join tbservicio in _dbcontext.Servicios on data.Idservicio equals tbservicio.Idservicio
                             group data by new { data.Idservicio, tbservicio.NombreServicio } into gr
                             select new VMServicios
                             {
                                 NombreServicio = gr.Key.NombreServicio ?? "sin nombre",
                                 Cantidad = gr.Count()
                             }).OrderByDescending(gr => gr.Cantidad).Take(5).ToList();

                if (Lista == null || !Lista.Any())
                {
                    return NotFound("No se encontraron datos para las reservas.");
                }

                return Ok(Lista);

            }
            catch (Exception ex)
            {
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }

        }

        public IActionResult Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            string nombreUsuario = "";

            if (claimuser.Identity.IsAuthenticated)
            {
                nombreUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.Name).
                        Select(c => c.Value).SingleOrDefault();
            }

            ViewData["nombreUsuario"] = nombreUsuario;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("IniciarSesion", "Inicio");
        }
    }
}
