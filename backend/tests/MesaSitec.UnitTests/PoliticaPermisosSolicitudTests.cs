using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;

namespace MesaSitec.UnitTests.Dominio;

public sealed class PoliticaPermisosSolicitudTests
{
    [Theory]
    [InlineData(AccionSolicitud.Asignar)]
    [InlineData(AccionSolicitud.Iniciar)]
    [InlineData(AccionSolicitud.Resolver)]
    [InlineData(AccionSolicitud.Cerrar)]
    [InlineData(AccionSolicitud.Reabrir)]
    [InlineData(AccionSolicitud.Cancelar)]
    public void Admin_PuedeEjecutarTodasLasAcciones(
        AccionSolicitud accion)
    {
        bool resultado =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    RolUsuario.Admin,
                    accion,
                    esPropietario: false);

        Assert.True(resultado);
    }

    [Fact]
    public void Agente_NoPuedeCancelar()
    {
        bool resultado =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    RolUsuario.Agente,
                    AccionSolicitud.Cancelar,
                    esPropietario: false);

        Assert.False(resultado);
    }

    [Fact]
    public void Solicitante_PuedeCerrarSolicitudPropia()
    {
        bool resultado =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    RolUsuario.Solicitante,
                    AccionSolicitud.Cerrar,
                    esPropietario: true);

        Assert.True(resultado);
    }

    [Theory]
    [InlineData(AccionSolicitud.Asignar)]
    [InlineData(AccionSolicitud.Iniciar)]
    [InlineData(AccionSolicitud.Resolver)]
    [InlineData(AccionSolicitud.Reabrir)]
    [InlineData(AccionSolicitud.Cancelar)]
    public void Solicitante_NoPuedeEjecutarAccionesAdministrativas(
        AccionSolicitud accion)
    {
        bool resultado =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    RolUsuario.Solicitante,
                    accion,
                    esPropietario: true);

        Assert.False(resultado);
    }

    [Fact]
    public void Solicitante_NoPuedeCerrarSolicitudAjena()
    {
        bool resultado =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    RolUsuario.Solicitante,
                    AccionSolicitud.Cerrar,
                    esPropietario: false);

        Assert.False(resultado);
    }
}