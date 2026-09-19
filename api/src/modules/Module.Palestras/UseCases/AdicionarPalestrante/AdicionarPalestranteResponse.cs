using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

public sealed record AdicionarPalestranteResponse(Guid PalestraId, Guid PessoaId, PalestrantePapel PalestrantePapel);
