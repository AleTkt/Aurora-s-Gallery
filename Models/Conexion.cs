using Microsoft.Extensions.Configuration;

namespace Catalogo_sis.Models
{
    public abstract class Conexion(IConfiguration configuracion)
    {
        protected readonly string? cadena_conexion = configuracion.GetConnectionString("DefaultConnection");
    }
}
