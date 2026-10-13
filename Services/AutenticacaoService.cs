using Biblioteca.Models;
using Biblioteca.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Services;

public class AutenticacaoService : IAutenticacaoService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AutenticacaoService(IUsuarioRepository usuarios, IPasswordHasher<Usuario> hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await _usuarios.ObterPorEmailAsync(email.Trim().ToLowerInvariant());
        return usuario != null && VerificarSenha(usuario, senha) ? usuario : null;
    }

    public bool VerificarSenha(Usuario usuario, string senha)
    {
        if (string.IsNullOrEmpty(usuario.SenhaHash)) return false; // usuário antigo, sem senha definida
        return _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha) != PasswordVerificationResult.Failed;
    }

    public string GerarHash(Usuario usuario, string senha) => _hasher.HashPassword(usuario, senha);
}
