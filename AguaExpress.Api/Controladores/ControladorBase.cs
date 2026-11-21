using Microsoft.AspNetCore.Mvc;

namespace AguaExpress.Api.Controladores;

public abstract class ControladorBase : ControllerBase
{
    protected int UsuarioId => int.Parse(User.FindFirst("sub")!.Value);
    protected IActionResult Error(int codigo, string mensaje) => StatusCode(codigo, new { mensaje });
}
