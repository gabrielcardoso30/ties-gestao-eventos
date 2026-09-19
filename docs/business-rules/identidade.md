# Regras de negócio — Identidade

Módulo em implementação; fonte: [`../spec/api-endpoints.md`](../spec/api-endpoints.md) e [`../spec/seguranca.md`](../spec/seguranca.md). Rota `api/v1/identidade`, schema `Identidade`, tabelas `Usuarios`, `Perfis`, `UsuarioPerfis`, `UsuarioClaims`, `UsuarioLogins`, `UsuarioTokens`, `PerfilClaims`.

## Entidades

**Usuário** (`Usuario : IdentityUser<Guid>`, auditável, emissor de eventos): `UsuarioNome`, `Email`, hash de senha, `UltimoAcessoEm`, campos de auditoria e soft delete. **Perfil** (`Perfil : IdentityRole<Guid>`): `Name`, `PerfilDescricao`. Um usuário pode ter vários perfis.

Usuário ≠ Pessoa: usuário opera o sistema; pessoa participa de eventos. Não há vínculo obrigatório entre eles hoje.

## Regras

| Código | Regra | Erro |
|---|---|---|
| RN-IDT-001 | Perfis conhecidos: `Administrador`, `Organizador`, `Participante` (`PerfisPadrao`), criados na subida se não existirem. | `422 Identidade.PerfilInvalido` ao atribuir outro |
| RN-IDT-002 | Administrador inicial criado na subida a partir de `Identidade:AdministradorInicial` (`Email`, `Nome`, `Senha`), apenas se não existir e apenas se houver senha configurada. Senha só por configuração/variável de ambiente. | — |
| RN-IDT-003 | Senha: mínimo 8 caracteres, com maiúscula, minúscula, dígito e símbolo. | `422 Identidade.SenhaFraca` |
| RN-IDT-004 | E-mail único entre usuários. | `409 Identidade.EmailJaCadastrado` |
| RN-IDT-005 | Cinco tentativas de login inválidas bloqueiam o usuário por 5 minutos. | `401 Identidade.UsuarioBloqueado` |
| RN-IDT-006 | Credenciais inválidas retornam sempre a mesma resposta genérica (não revelar se o e-mail existe). | `401 Identidade.CredenciaisInvalidas` |
| RN-IDT-007 | Login bem-sucedido emite JWT HS256 com `sub`, `name`, `email`, `role` (uma por perfil), `jti`, expiração `Jwt:ExpirationMinutes`; atualiza `UltimoAcessoEm`; emite `UsuarioAutenticado`. | — |
| RN-IDT-008 | Registrar usuário exige `Administracao`, atribui um ou mais perfis válidos e emite `UsuarioRegistrado`. | `403` |
| RN-IDT-009 | Alterar perfis de um usuário exige `Administracao`. | `404 Identidade.UsuarioNaoEncontrado` |
| RN-IDT-010 | `GET /usuarios/me` devolve os dados do usuário autenticado a partir do token. | `401` |
| RN-IDT-011 | Usuários podem ser inativados (`EstaAtivo=false`) ou excluídos logicamente; usuário inativo/excluído não autentica. | `401 Identidade.CredenciaisInvalidas` |
| RN-IDT-012 | Login tem limite de requisições mais rígido que o global (ex.: 10/min por IP). | `429` |

## Políticas derivadas

| Política | Perfis | Uso |
|---|---|---|
| `Gestao` | Administrador, Organizador | escrita em Locais, Pessoas, Eventos, Palestras; registrar presença |
| `Administracao` | Administrador | usuários, perfis, auditoria |
| autenticado | qualquer | leituras, inscrição, emissão de certificado |

## Eventos publicados

`UsuarioRegistrado(UsuarioId, UsuarioEmail, UsuarioNome)`, `UsuarioAutenticado(UsuarioId, UsuarioEmail)`.

## Endpoints

| Método | Rota | Política |
|---|---|---|
| POST | `/api/v1/identidade/sessoes` | anônimo |
| POST | `/api/v1/identidade/usuarios` | Administracao |
| GET | `/api/v1/identidade/usuarios/me` | autenticado |
| GET | `/api/v1/identidade/usuarios` | Administracao |
| PUT | `/api/v1/identidade/usuarios/{id}/perfis` | Administracao |
