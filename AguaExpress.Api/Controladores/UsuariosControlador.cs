using AguaExpress.Api.Datos;
using AguaExpress.Api.Dominio;
using AguaExpress.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AguaExpress.Api.Controladores;

[ApiController, Route("usuarios"), Authorize(Roles = "Administrador")]
public class UsuariosControlador(AguaExpressDb db) : ControladorBase
{
    /// <summary>Trabajadores = vendedores y administradores.</summary>
    [HttpGet("trabajadores")]
    public async Task<List<UsuarioDto>> Trabajadores() =>
        await db.Usuarios.Where(u => u.Rol != RolUsuario.Cliente).OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioDto(u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Rol, u.Activo)).ToListAsync();

    [HttpPost("trabajadores")]
    public async Task<IActionResult> Crear(NuevoUsuarioDto dto)
    {
        if (dto.Rol == RolUsuario.Cliente) return Error(400, "Los clientes se registran desde la app.");
        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto) || !correo.Contains('@') || dto.Contrasena.Length < 6)
            return Error(400, "Datos incompletos. La contraseña debe tener al menos 6 caracteres.");
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo)) return Error(409, "Ese correo ya está registrado.");

        var u = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(), Correo = correo, Telefono = dto.Telefono.Trim(),
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena), Rol = dto.Rol
        };
        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        return Ok(new UsuarioDto(u.Id, u.NombreCompleto, u.Correo, u.Telefono, u.Rol, u.Activo));
    }

    [HttpPut("{id:int}/activo")]
    public async Task<IActionResult> AlternarActivo(int id)
    {
        var u = await db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();
        if (u.Id == UsuarioId) return Error(400, "No puedes desactivar tu propia cuenta.");
        u.Activo = !u.Activo;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
