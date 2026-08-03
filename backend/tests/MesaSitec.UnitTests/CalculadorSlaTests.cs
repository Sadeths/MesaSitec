using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;

namespace MesaSitec.UnitTests.Dominio;

public sealed class CalculadorSlaTests
{
    private static readonly DateTime FechaBase =
        new(
            year: 2026,
            month: 1,
            day: 15,
            hour: 8,
            minute: 0,
            second: 0,
            kind: DateTimeKind.Utc);

    [Fact]
    public void Critica_AplicaFactorDeCeroPuntoCinco()
    {
        DateTime resultado =
            CalculadorSla.CalcularFechaLimite(
                FechaBase,
                slaHoras: 8,
                PrioridadSolicitud.Critica);

        DateTime esperado =
            FechaBase.AddHours(4);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Alta_AplicaFactorDeCeroPuntoSetentaYCinco()
    {
        DateTime resultado =
            CalculadorSla.CalcularFechaLimite(
                FechaBase,
                slaHoras: 8,
                PrioridadSolicitud.Alta);

        DateTime esperado =
            FechaBase.AddHours(6);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Media_AplicaFactorDeUno()
    {
        DateTime resultado =
            CalculadorSla.CalcularFechaLimite(
                FechaBase,
                slaHoras: 8,
                PrioridadSolicitud.Media);

        DateTime esperado =
            FechaBase.AddHours(8);

        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Baja_AplicaFactorDeDos()
    {
        DateTime resultado =
            CalculadorSla.CalcularFechaLimite(
                FechaBase,
                slaHoras: 8,
                PrioridadSolicitud.Baja);

        DateTime esperado =
            FechaBase.AddHours(16);

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-20)]
    public void SlaNoPositivo_LanzaExcepcion(
        int slaHoras)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CalculadorSla.CalcularFechaLimite(
                FechaBase,
                slaHoras,
                PrioridadSolicitud.Media));
    }
}