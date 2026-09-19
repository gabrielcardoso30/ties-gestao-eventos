# ADR 0006 — ASP.NET Core Identity sobrescrito com JWT próprio

Status: Aceita · Data: 2026-09

## Contexto

O requisito é usar o Identity do ASP.NET Core para autenticação e autorização, mas as tabelas padrão (`AspNetUsers`, chaves `string`, schema `public`) violam as convenções do projeto (PascalCase em pt-BR, `Guid` v7, schema por módulo, auditoria e soft delete). A API é consumida por um SPA e precisa de autenticação stateless.

## Decisão

- O módulo **Identidade** usa o Identity como biblioteca de usuários/senhas/perfis com classes próprias: `Usuario : IdentityUser<Guid>, IEntidadeAuditavel, IEmissorDeEventos` (tabela `Usuarios`), `Perfil : IdentityRole<Guid>` (`Perfis`), `UsuarioPerfil`, `UsuarioClaim`, `UsuarioLogin`, `UsuarioToken`, `PerfilClaim`, todas no schema `Identidade`. `IdentidadeDbContext : IdentityDbContext<...>` replica o essencial do `ModuleDbContext` (schema, `OutboxMessages`, filtro de soft delete em `Usuario`) porque não há herança múltipla.
- Não se usa cookie nem Identity UI. `POST /api/v1/identidade/sessoes` valida credenciais com o `SignInManager`/`UserManager` e **emite um JWT HS256** com `JwtOptions` (`Shared.WebHost.Security`): claims `sub`, `name`, `email`, `role`, `jti`; expiração `Jwt:ExpirationMinutes`.
- A validação do token é única para todos os módulos (`AddJwtBearer` em `Shared.WebHost`), e a autorização usa políticas por perfil (`Politicas.Gestao`, `Politicas.Administracao`) definidas em `Shared.Contracts`.
- Perfis e administrador inicial são semeados na subida por hosted service do módulo, a partir de `PerfisPadrao` e `Identidade:AdministradorInicial`.

## Consequências

Positivas:

- reaproveita hashing, lockout, política de senha e gestão de perfis maduros do Identity;
- tokens stateless escalam horizontalmente e servem a qualquer host futuro com a mesma chave;
- tabelas seguem as convenções e a auditoria automática cobre usuários.

Negativas:

- revogação imediata de token não existe (só expiração; mitigação futura: lista de `jti` revogados ou refresh tokens curtos);
- chave simétrica compartilhada entre hosts; ao extrair módulos, considerar chave assimétrica (RS256) com JWKS;
- `IdentidadeDbContext` duplica parte do `ModuleDbContext`; qualquer evolução do base precisa ser refletida lá.

## Alternativas consideradas

- **Identity padrão com cookies**: não serve a SPA + API stateless; descartado.
- **Provedor externo (Entra ID, Keycloak, Auth0)**: ótimo para produção corporativa, mas o requisito pede Identity e o projeto deve subir com um comando; pode ser integrado depois como esquema adicional.
- **Tabela de usuários própria sem Identity**: reinventar hashing e lockout; descartado.
