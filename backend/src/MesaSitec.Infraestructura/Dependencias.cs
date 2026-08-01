using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Infraestructura.Autenticacion;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MesaSitec.Infraestructura;

public static class Dependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        string cadenaConexion,
        string jwtSecret)
    {
        servicios.AddDbContext<MesaSitecDbContext>(opciones =>
            opciones.UseSqlite(cadenaConexion));

        servicios.AddSingleton<IGeneradorJwt>(
            new GeneradorJwt(jwtSecret));

        servicios.AddScoped<ILoginServicio, LoginServicio>();

        servicios.AddScoped<IPerfilServicio, PerfilServicio>();

        return servicios;
    }
}