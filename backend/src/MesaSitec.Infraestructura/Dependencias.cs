using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Infraestructura.Autenticacion;
using MesaSitec.Infraestructura.Categorias;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Seguridad;
using MesaSitec.Infraestructura.Solicitudes;
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
        servicios.AddDbContext<MesaSitecDbContext>(
            opciones =>
                opciones.UseSqlite(cadenaConexion));

        servicios.AddSingleton<IGeneradorJwt>(
            new GeneradorJwt(jwtSecret));

        servicios.AddScoped<
            ILoginServicio,
            LoginServicio>();

        servicios.AddScoped<
            IPerfilServicio,
            PerfilServicio>();

        servicios.AddScoped<
            ICategoriaServicio,
            CategoriaServicio>();

        servicios.AddScoped<
            ISolicitudConsultaServicio,
            SolicitudConsultaServicio>();

        servicios.AddScoped<
            ISolicitudCreacionServicio,
            SolicitudCreacionServicio>();

        servicios.AddScoped<
            ISolicitudDetalleServicio,
            SolicitudDetalleServicio>();

        servicios.AddScoped<
            ISolicitudEdicionServicio,
            SolicitudEdicionServicio>();

        servicios.AddScoped<
            ISolicitudTransicionServicio,
            SolicitudTransicionServicio>();

        return servicios;
    }
}