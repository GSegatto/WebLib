namespace Biblioteca.Services;

/// <summary>Violação de regra de negócio com mensagem pronta para o usuário final.</summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}
