# Changelog

Todas as mudanças notáveis deste projeto serão documentadas neste arquivo.
O formato é baseado em [Keep a Changelog](https://keepachangelog.com/) e o
versionamento segue [SemVer](https://semver.org/).

## [Unreleased] — 2026-07-23

> Auditoria de segurança, refatoração DRY e migração do banco de dados.
> Este release **não usa Supabase**: o banco anterior era PostgreSQL hospedado
> em Render.com; agora é PostgreSQL **auto-hospedado via Docker** na máquina/servidor.

### Migration — Render.com PostgreSQL → PostgreSQL local (Docker)

- **Adicionado** `docker-compose.yml` na raiz, sobe PostgreSQL 16-alpine com
  volume nomeado (`cheerbr_pgdata`) e healthcheck (`pg_isready`).
  Inclui também serviço `api` que builda o backend em container, conectando ao
  `postgres` via `DATABASE_URL`.
- **Adicionado** `.env.example` na raiz com todas as variáveis de ambiente
  (`POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `DATABASE_URL`,
  `UPLOADS_PATH`, `CORS_ALLOWED_ORIGINS`, `VITE_API_URL`).
- **Adicionado** `backend/.../Migrations/baseline.sql` — script SQL
  **idempotente** gerado pelo EF Core que reproduz as 9 migrations existentes.
- **Adicionado** `backend/.../Migrations/baseline_clean.sql` — schema
  consolidado (squash) para bancos **novos**, que cria as 3 tabelas
  (`Teams`, `CompetitionResults`, `Championships`) em um único passo e marca
  todas as migrations EF como já aplicadas (evita replay).
- **Alterado** `appsettings.json` e `appsettings.Development.json`: removida a
  connection string hardcoded do Render e a chave `Redis` morta. Agora apontam
  para `localhost:5432` como fallback; em produção a string deve vir da env
  var `DATABASE_URL`.
- **Alterado** `Program.cs`: lê `DATABASE_URL` (env) primeiro, depois
  `ConnectionStrings:DefaultConnection`. Falha com erro explícito se nenhuma
  das duas estiver configurada.
- **Refatorado** `AppDbContextFactory.cs`: o factory de design-time (usado por
  `dotnet ef`) agora lê `DATABASE_URL` → `ConnectionStrings__DefaultConnection`
  → fallback localhost, em vez de apontar cegamente para o Render.
- **Documentado** procedimento de dump/restore no `README.md` e no
  `DOCUMENTATION.md` (`pg_dump` do Render → `pg_restore` no local).

### Security

- **Removida** credential leak: a senha `qF6z…WIq5` do Render estava hardcoded
  em `appsettings.json`, `appsettings.Development.json` e
  `AppDbContextFactory.cs`. Todas as 3 ocorrências foram substituídas por
  placeholders/env vars.
- **Adicionado** `.gitignore` entries: `.env`, `bin/`, `obj/`, `**/wwwroot/uploads/*`
  (binários de upload), `.pgdata/`.
- **Removido do tracking do git** os 57 logos PNG que estavam commitados em
  `backend/Cheer.Api/wwwroot/uploads/` (`git rm --cached` — arquivos
  preservados no disco para migração; binários de upload não devem ser
  versionados).
- **Alterado** `backend/Dockerfile`: adicionado `USER app` (a imagem
  `aspnet:10.0` já provisiona o usuário `app` sem privilégios de root) e
  `RUN chown app:app /var/lib/cheerbr/uploads`. Container agora roda como
  não-root.
- **Adicionado** `[RequestSizeLimit(5_000_000)]` em `POST /api/teams/{id}/logo`
  (5 MB).
- **Adicionada** validação defensiva de upload de logo no
  `TeamsController.UploadLogo`:
  - Content-Type allowlist: `image/jpeg`, `image/png`, `image/webp`, `image/gif`.
  - Extensão allowlist: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`.
  - Geração de nome de arquivo server-side (`{id}_{Guid}{ext}`) — não confia no
    nome enviado pelo cliente (proteção contra path traversal).
  - `UPLOADS_PATH` configurável via env var (volume persistente no host/Docker).

- **Adicionada** validação estrita de entrada com `System.ComponentModel.DataAnnotations`
  em todos os DTOs de entrada:
  - `CreateTeamDto` / `UpdateTeamDto`: `Required`, `StringLength`, `Range`,
    `Url` em `LogoUrl`, `Range(1,6)` em `Nivel`.
  - `CreateCompetitionResultDto` / `UpdateCompetitionResultDto`: `Range(1900,2100)`
    em `Ano`, `Range(1,6)` em `Nivel`, `Range(1, int.MaxValue)` em `Colocacao`,
    `StringLength` em textos.
  - `CreateChampionshipDto`: `Required`, `StringLength(200)`.
  - O `[ApiController]` retorna automaticamente `400 ProblemDetails` quando a
    validação falha.

- **CORS** agora configurável via env var `CORS_ALLOWED_ORIGINS` (CSV). Quando
  ausente mantém o comportamento `AllowAnyOrigin` legado; quando definida,
  aplica `WithOrigins` + wildcard subdomains (mais seguro para produção).

### Removed — Dead Code Elimination

- **Backend**: removida referência NuGet `Microsoft.EntityFrameworkCore.Sqlite`
  de `Cheer.Infrastructure.csproj` (provider SQLite nunca usado em runtime — o
  app usa exclusivamente Npgsql).
- **Backend**: removida chave `"Redis": "localhost:6379"` dos `appsettings`
  (Redis jamais referenciado no código).
- **Frontend**: removidos os 3 métodos `api.getTeam`, `api.getRanking`,
  `api.getStatsOverview` de `src/lib/api.ts` — **nenhum chamador** existia
  (ranking e stats são calculados client-side a partir de `useTeams()`).
- **Frontend**: removido o tipo `TeamFormData` de `src/lib/types.ts` —
  importado em 0 locais (cada formulário define seu próprio `z.infer`).
- **Frontend**: removidos imports não usados em `src/routes/equipes_.$id.tsx`
  (`useQueryClient`, `api`, casts `as unknown as Record<string, unknown>`).

### Changed — Refatoração DRY

- **Frontend**: `src/lib/api.ts` reescrito com um helper interno `request()`
  que centraliza `fetch` + headers JSON + tratamento de `!res.ok` (com body do
  erro exposto na mensagem) + parsing de `204 No Content`. Elimina ~14 cópias
  do mesmo boilerplate. Tipagem forte: `ResultPayload`,
  `CreateTeamPayload`, `UpdateTeamPayload`.
- **Frontend**: `src/lib/teams-store.ts` agora exporta `useInvalidateAll()`
  (substitui 4 cópias do bloco triplo
  `invalidateQueries(["teams"],["ranking"],["stats"])`).
- **Frontend**: `useTeamResults` estendido com `updateResult` e `deleteResult`
  (mutation hooks). `src/routes/equipes_.$id.tsx` não chama mais `api.*`
  diretamente nem manipula cache manualmente — tudo passa pelo hook.
- **Frontend**: adicionado helper genérico `countBy<T>` em `src/lib/utils.ts`;
  `dashboard.tsx` reduziu 4 blocos `useMemo` quase idênticos para 4 chamadas de
  1 linha.
- **Frontend**: adicionada constante `NIVEIS` em `src/lib/constants.ts`
  (elimina 2 cópias de `Array.from({ length: NIVEL_MAX }, (_, i) => i + 1)`
  em `equipes.index.tsx` e `equipes_.$id.tsx`).
- **Frontend**: `src/lib/api.ts` agora lê `import.meta.env.VITE_API_URL`
  (fallback `http://localhost:10000/api`). Antes a URL era hardcoded para
  `cheerbr-2.onrender.com`.
- **Frontend**: tipo `Team` ganhou campo opcional `results?: CompetitionResult[]`
  — elimina cast legado `(team as Record<string, unknown>).results`.
- **Frontend**: tipo `CompetitionResult` ganhou `teamId?` para refletir o DTO
  C# exatamente.

### Documentation

- **Atualizado** `README.md` com visão geral, stack, instruções para subir o
  PostgreSQL local via `docker compose`, pré-requisitos, e guia passo a passo
  para dev e deploy.
- **Criado** `DOCUMENTATION.md` com contexto/regras de negócio, arquitetura
  técnica, fluxo de dados, ERD (entidades) e mapeamento completo de endpoints.

### Known Limitations / Próximos passos

- **Sem autenticação**: a API continua pública (sem JWT/Identity). Implementar
  auth JWT + BCrypt + RBAC é scope separado — a lacuna foi **documentada**
  (`[Authorize]` stub não foi adicionado para não sugerir falsa proteção).
- **Logos em disco local**: persistem via volume Docker (`cheerbr_uploads`),
  mas NÃO há estratégia de limpeza de logos órfãos quando uma equipe é
  removida ou atualiza o logo. Migrar para object storage (S3/R2) é recomendado
  para produção multi-instância.
- **Seed data**: o backend não possui `HasData` em `AppDbContext.OnModelCreating`.
  As 89 equipes vivem em `src/data/teams.seed.json` (frontend-only); para popular
  o Postgres local de fato é preciso importar o dump do Render.
