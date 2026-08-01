using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MesaSitec.Aplicacion.Autenticacion;
using Microsoft.IdentityModel.Tokens;

namespace MesaSitec.Infraestructura.Seguridad;

public sealed class GeneradorJwt : IGeneradorJwt
{
    private readonly string _secreto;

    public GeneradorJwt(string secreto)
    {
        _secreto = secreto;
    }

    public string Generar(DatosToken datos)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                datos.UsuarioId.ToString()),
            
            new(
                "tenantId",
                datos.TenantId.ToString()),
            
            new(
                "rol",
                datos.Rol),
            
            new(
                JwtRegisteredClaimNames.Email,
                datos.Email),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var llave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_secreto));
        
        var credenciales = new SigningCredentials(
                llave,
                SecurityAlgorithms.HmacSha256);
        
        DateTime fechaExpiracion = DateTime.UtcNow.AddSeconds(
            JwtConfiguracion.ExpiracionSegundos);
        
        var token = new JwtSecurityToken(
            issuer: JwtConfiguracion.Emisor,
            audience: JwtConfiguracion.Audiencia,
            claims: claims,
            expires: fechaExpiracion,
            signingCredentials: credenciales);
        
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}