using System.Security.Claims;
using System.Text;
using AguaExpress.Api.Dominio;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AguaExpress.Api.Servicios;

public class ServicioToken(IConfiguration cfg)
{
    public string Generar(Usuario u)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Clave"]!));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", u.Id.ToString()), new Claim("role", u.Rol.ToString())]),
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
