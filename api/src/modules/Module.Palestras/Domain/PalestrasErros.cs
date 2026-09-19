using Shared.Http.Results;

namespace Module.Palestras.Domain;

public static class PalestrasErros
{
    public static readonly Error PalestraNaoEncontrada = Error.NotFound("Palestras.PalestraNaoEncontrada", "Palestra não encontrada.");
    public static readonly Error EventoNaoEncontrado = Error.BusinessRule("Palestras.EventoNaoEncontrado", "O evento informado não existe.");
    public static readonly Error EventoNaoAceitaPalestras = Error.BusinessRule("Palestras.EventoNaoAceitaPalestras", "O evento está encerrado ou cancelado e não aceita palestras.");
    public static readonly Error PeriodoForaDoEvento = Error.BusinessRule("Palestras.PeriodoForaDoEvento", "O período da palestra deve estar dentro do período do evento.");
    public static readonly Error SalaNaoEncontrada = Error.BusinessRule("Palestras.SalaNaoEncontrada", "A sala informada não existe ou está inativa.");
    public static readonly Error SalaNaoPertenceAoLocal = Error.BusinessRule("Palestras.SalaNaoPertenceAoLocal", "A sala informada não pertence ao local do evento.");
    public static readonly Error SalaOcupada = Error.Conflict("Palestras.SalaOcupada", "Já existe outra palestra nesta sala no horário informado.");
    public static readonly Error PessoaNaoEncontrada = Error.BusinessRule("Palestras.PessoaNaoEncontrada", "Uma ou mais pessoas informadas como palestrantes não existem.");
    public static readonly Error PalestranteJaVinculado = Error.Conflict("Palestras.PalestranteJaVinculado", "Esta pessoa já é palestrante desta palestra.");
    public static readonly Error PalestranteNaoEncontrado = Error.NotFound("Palestras.PalestranteNaoEncontrado", "Palestrante não encontrado nesta palestra.");
    public static readonly Error PalestraPrecisaDePalestrante = Error.BusinessRule("Palestras.PalestraPrecisaDePalestrante", "Uma palestra precisa manter ao menos um palestrante.");
    public static readonly Error ConteudoNaoEncontrado = Error.NotFound("Palestras.ConteudoNaoEncontrado", "Conteúdo não encontrado nesta palestra.");
    public static readonly Error ParticipanteNaoInscrito = Error.BusinessRule("Palestras.ParticipanteNaoInscrito", "A pessoa não possui inscrição confirmada no evento desta palestra.");
    public static readonly Error PresencaJaRegistrada = Error.Conflict("Palestras.PresencaJaRegistrada", "A presença desta pessoa já foi registrada nesta palestra.");
    public static readonly Error PresencaNaoRegistrada = Error.BusinessRule("Palestras.PresencaNaoRegistrada", "A pessoa não possui presença registrada nesta palestra.");
    public static readonly Error PalestraNaoEncerrada = Error.BusinessRule("Palestras.PalestraNaoEncerrada", "O certificado só pode ser emitido após o término da palestra.");
    public static readonly Error CertificadoNaoEncontrado = Error.NotFound("Palestras.CertificadoNaoEncontrado", "Certificado não encontrado.");
}
