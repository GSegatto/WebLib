using Biblioteca.Models;

namespace Biblioteca.Services;

public interface IAutenticacaoService
{
    Task<Usuario?> AutenticarAsync(string email, string senha);
    bool VerificarSenha(Usuario usuario, string senha);
    string GerarHash(Usuario usuario, string senha);
}
