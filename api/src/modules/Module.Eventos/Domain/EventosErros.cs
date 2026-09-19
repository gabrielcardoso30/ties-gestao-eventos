using Shared.Http.Results;

namespace Module.Eventos.Domain;

public static class EventosErros
{
    public static readonly Error EventoNaoEncontrado = Error.NotFound("Eventos.EventoNaoEncontrado", "Evento não encontrado.");
    public static readonly Error LocalNaoEncontrado = Error.BusinessRule("Eventos.LocalNaoEncontrado", "O local informado não existe.");
    public static readonly Error FormatoInconsistente = Error.BusinessRule("Eventos.FormatoInconsistente", "Os dados de local e link remoto não são compatíveis com o formato do evento.");
    public static readonly Error EventoSemPalestras = Error.BusinessRule("Eventos.EventoSemPalestras", "O evento precisa ter ao menos uma palestra para ser publicado.");
    public static readonly Error MotivoCancelamentoObrigatorio = Error.BusinessRule("Eventos.MotivoCancelamentoObrigatorio", "Informe o motivo do cancelamento.");
    public static readonly Error TransicaoSituacaoInvalida = Error.BusinessRule("Eventos.TransicaoSituacaoInvalida", "Transição de situação não permitida para o evento.");
    public static readonly Error EventoNaoPodeSerAlterado = Error.BusinessRule("Eventos.EventoNaoPodeSerAlterado", "Somente eventos em rascunho ou publicados podem ser alterados.");
    public static readonly Error EventoNaoPodeSerExcluido = Error.BusinessRule("Eventos.EventoNaoPodeSerExcluido", "Somente eventos em rascunho ou cancelados podem ser excluídos.");
    public static readonly Error EventoNaoAceitaInscricoes = Error.BusinessRule("Eventos.EventoNaoAceitaInscricoes", "O evento não aceita inscrições na situação atual.");
    public static readonly Error PessoaNaoEncontrada = Error.BusinessRule("Eventos.PessoaNaoEncontrada", "A pessoa informada não existe.");
    public static readonly Error PessoaJaInscrita = Error.Conflict("Eventos.PessoaJaInscrita", "A pessoa já possui inscrição confirmada neste evento.");
    public static readonly Error CapacidadeEsgotada = Error.BusinessRule("Eventos.CapacidadeEsgotada", "A capacidade do evento foi atingida.");
    public static readonly Error InscricaoNaoEncontrada = Error.NotFound("Eventos.InscricaoNaoEncontrada", "Inscrição não encontrada neste evento.");
    public static readonly Error InscricaoJaCancelada = Error.BusinessRule("Eventos.InscricaoJaCancelada", "A inscrição já está cancelada.");
}
