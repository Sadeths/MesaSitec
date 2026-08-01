namespace MesaSitec.Aplicacion.Autenticacion;

public interface IGeneradorJwt
{
     string Generar(DatosToken datos);
}