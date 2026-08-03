using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;

namespace MesaSitec.UnitTests.Dominio;

public sealed class MaquinaEstadosSolicitudTests
{
    [Fact]
    public void Nueva_Asignar_CambiaAAsignada()
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                EstadoSolicitud.Nueva,
                AccionSolicitud.Asignar,
                out EstadoSolicitud destino);

        Assert.True(resultado);
        Assert.Equal(
            EstadoSolicitud.Asignada,
            destino);
    }

    [Fact]
    public void Asignada_Iniciar_CambiaAEnProceso()
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                EstadoSolicitud.Asignada,
                AccionSolicitud.Iniciar,
                out EstadoSolicitud destino);

        Assert.True(resultado);
        Assert.Equal(
            EstadoSolicitud.EnProceso,
            destino);
    }

    [Fact]
    public void EnProceso_Resolver_CambiaAResuelta()
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                EstadoSolicitud.EnProceso,
                AccionSolicitud.Resolver,
                out EstadoSolicitud destino);

        Assert.True(resultado);
        Assert.Equal(
            EstadoSolicitud.Resuelta,
            destino);
    }

    [Fact]
    public void Resuelta_Cerrar_CambiaACerrada()
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                EstadoSolicitud.Resuelta,
                AccionSolicitud.Cerrar,
                out EstadoSolicitud destino);

        Assert.True(resultado);
        Assert.Equal(
            EstadoSolicitud.Cerrada,
            destino);
    }

    [Fact]
    public void Resuelta_Reabrir_CambiaAEnProceso()
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                EstadoSolicitud.Resuelta,
                AccionSolicitud.Reabrir,
                out EstadoSolicitud destino);

        Assert.True(resultado);
        Assert.Equal(
            EstadoSolicitud.EnProceso,
            destino);
    }

    [Theory]
    [InlineData(
        EstadoSolicitud.Nueva,
        AccionSolicitud.Resolver)]

    [InlineData(
        EstadoSolicitud.Cerrada,
        AccionSolicitud.Reabrir)]

    [InlineData(
        EstadoSolicitud.Cancelada,
        AccionSolicitud.Iniciar)]

    [InlineData(
        EstadoSolicitud.Asignada,
        AccionSolicitud.Cerrar)]
    public void TransicionNoPermitida_DevuelveFalse(
        EstadoSolicitud estadoActual,
        AccionSolicitud accion)
    {
        bool resultado =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                estadoActual,
                accion,
                out EstadoSolicitud destino);

        Assert.False(resultado);
        Assert.Equal(estadoActual, destino);
    }
}