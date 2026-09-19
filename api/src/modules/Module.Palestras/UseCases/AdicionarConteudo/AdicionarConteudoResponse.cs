using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.AdicionarConteudo;

public sealed record AdicionarConteudoResponse(Guid Id, Guid PalestraId, string ConteudoTitulo, ConteudoTipo ConteudoTipo, string ConteudoUrl);
