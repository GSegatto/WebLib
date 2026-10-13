using System.Security.Claims;
using Biblioteca.Models;

namespace Biblioteca.Extensions;

public static class UserExtensions
{
    public static int UsuarioId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static bool EhAdmin(this ClaimsPrincipal user) => user.IsInRole(Perfis.Admin);
}
