using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.App.Models;

public class LoginDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRespuestaDto
{
    public string Token { get; set; } = "";
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Rol { get; set; } = "";
}

public class ErrorRespuesta
{
    public string? Mensaje { get; set; }
}