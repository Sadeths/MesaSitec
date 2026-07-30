using System.Net;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MesaSitec.Infraestructura;

public static class Dependencias
{
    public static IServiceCollection AgregarInfraEstructura(
        this IServiceCollection servicios,
        string cadenaConexion)
    {
        servicios.AddDbContext<MesaSitecDbContext>(opciones =>
            opciones.UseSqlite(cadenaConexion));

        return servicios;
    }
}