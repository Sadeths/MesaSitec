using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Infraestructura.Autenticacion;
using MesaSitec.Infraestructura.Categorias;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Infraestructura.Solicitudes;

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



        return servicios;
    }
}