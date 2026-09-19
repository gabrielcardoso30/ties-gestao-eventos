using Shared.Http.Results;

namespace Module.Auditoria.Domain;

public static class AuditoriaErros
{
    public static readonly Error RegistroNaoEncontrado = Error.NotFound("Auditoria.RegistroNaoEncontrado", "Registro de auditoria não encontrado.");
}
