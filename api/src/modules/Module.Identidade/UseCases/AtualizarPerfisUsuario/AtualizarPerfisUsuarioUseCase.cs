using Microsoft.EntityFrameworkCore;
using Module.Identidade.Domain;
using Module.Identidade.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

/// <summary>Substitui o conjunto de perfis do usuário (diff em <c>UsuarioPerfis</c>: remove os ausentes, adiciona os novos). Lista vazia remove todos.</summary>
internal sealed class AtualizarPerfisUsuarioUseCase(IdentidadeDbContext db) : IUseCase<AtualizarPerfisUsuarioRequest, AtualizarPerfisUsuarioResponse>
{
    public async Task<Result<AtualizarPerfisUsuarioResponse>> HandleAsync(AtualizarPerfisUsuarioRequest request, CancellationToken cancellationToken)
    {
        var perfis = PerfisValidos.Normalizar(request.Perfis);
        if (perfis.IsFailure)
        {
            return perfis.Error;
        }

        var usuario = await db.Usuarios
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarUsuario")
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            return IdentidadeErros.UsuarioNaoEncontrado;
        }

        var desejados = perfis.Value;
        var perfisExistentes = await db.Perfis
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarPerfis")
            .AsNoTracking()
            .Where(p => desejados.Contains(p.Name!))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken);
        var naoEncontrado = desejados.FirstOrDefault(d => perfisExistentes.All(p => p.Name != d));
        if (naoEncontrado is not null)
        {
            return IdentidadeErros.PerfilInvalido(naoEncontrado);
        }

        var vinculosAtuais = await db.UsuarioPerfis
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarVinculos")
            .Where(up => up.UserId == usuario.Id)
            .ToListAsync(cancellationToken);

        var idsDesejados = perfisExistentes.Select(p => p.Id).ToHashSet();
        var remover = vinculosAtuais.Where(v => !idsDesejados.Contains(v.RoleId)).ToList();
        var adicionar = idsDesejados.Where(id => vinculosAtuais.All(v => v.RoleId != id))
            .Select(id => new UsuarioPerfil { UserId = usuario.Id, RoleId = id })
            .ToList();

        // Perfis mudaram: renova o ConcurrencyStamp para que o usuário apareça na trilha de auditoria (EntidadeAlterada).
        if (remover.Count > 0 || adicionar.Count > 0)
        {
            usuario.ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.UsuarioPerfis.RemoveRange(remover);
            db.UsuarioPerfis.AddRange(adicionar);
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarPerfisUsuarioResponse(usuario.Id, desejados));
        }, cancellationToken);
    }
}
