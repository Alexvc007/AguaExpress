using AguaExpress.Api.Datos;
using AguaExpress.Api.Dominio;
using AguaExpress.Api.Dtos;
using AguaExpress.Api.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Controladores;

[ApiController]
public class AuthControlador(AguaExpressDb db, ServicioToken tokens) : ControladorBase
{
    [HttpPost("auth/registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto) || !correo.Contains('@') || dto.Contrasena.Length < 6)
            return Error(400, "Completa todos los datos. La contraseña debe tener al menos 6 caracteres.");
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
            return Error(409, "Ya existe una cuenta con ese correo.");

        var u = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(), Correo = correo, Telefono = dto.Telefono.Trim(),
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena), Rol = RolUsuario.Cliente
        };
        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        return Ok(new SesionDto(tokens.Generar(u), u.Id, u.NombreCompleto, u.Rol.ToString()));
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo);
        if (u is null || !BCrypt.Net.BCrypt.Verify(dto.Contrasena, u.ContrasenaHash))
            return Error(401, "Correo o contraseña incorrectos.");
        if (!u.Activo) return Error(403, "Tu cuenta está desactivada. Contacta al administrador.");
        return Ok(new SesionDto(tokens.Generar(u), u.Id, u.NombreCompleto, u.Rol.ToString()));
    }

    [HttpPut("usuarios/token-fcm"), Authorize]
    public async Task<IActionResult> GuardarTokenFcm(TokenFcmDto dto)
    {
        var u = await db.Usuarios.FindAsync(UsuarioId);
        if (u is null) return NotFound();
        u.TokenFcm = dto.Token;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
