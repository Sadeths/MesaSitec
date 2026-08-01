using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Servicios;

public static class CalculadorSla
{
    public static DateTime CalcularFechaLimite(
        DateTime fechaCreacion,
        int slaHoras,
        PrioridadSolicitud prioridad)
    {
        if (slaHoras <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slaHoras),
                "Las horas del SLA deben ser mayores que cero.");
        }

        double factor = prioridad switch
        {
            PrioridadSolicitud.Critica => 0.5,
            PrioridadSolicitud.Alta => 0.75,
            PrioridadSolicitud.Media => 1.0,
            PrioridadSolicitud.Baja => 2.0,

            _ => throw new ArgumentOutOfRangeException(
                nameof(prioridad),
                prioridad,
                "La prioridad indicada no es válida.")
        };

        return fechaCreacion.AddHours(slaHoras * factor);
    }
}