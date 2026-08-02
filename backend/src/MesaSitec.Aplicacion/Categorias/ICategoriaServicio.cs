namespace MesaSitec.Aplicacion.Categorias;

public interface ICategoriaServicio
{
    Task<IReadOnlyList<CategoriaRespuesta>> ListarActivasAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}